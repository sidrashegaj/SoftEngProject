import { Component, OnDestroy, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { Campground } from '../../models/campground.model';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { CampgroundService } from '../../services/campground.service';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import * as mapboxgl from 'mapbox-gl';
import { environment } from '../../../environments/environment';
import { Review } from '../../models/review.model';
import { ReviewService } from '../../services/review.service';
import { AuthService } from '../../services/auth.service';
import L from 'leaflet';
import { HttpHeaders } from '@angular/common/http';
import { FlashMessageService } from '../../services/flash-message.service';

@Component({
  selector: 'app-campground-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './campground-detail.component.html',
  styleUrls: ['./campground-detail.component.css'],
})
export class CampgroundDetailComponent implements OnInit, OnDestroy {
  campground: Campground = {
    campgroundId: 0,
    title: '',
    description: '',
    location: '',
    images: [],
    price: 0,
    author: { _id: 0, username: '' },
    reviews: [],
    geometry: { coordinates: [0, 0] },
    name: '',
    longitude: undefined,
    latitude: undefined
  };
  map!: mapboxgl.Map;
  id: any;
  randomImageUrl: string = '';
  reviews: Review[] = [];
  newReview: Review = {
    reviewId: 0,
    text: '',
    timestamp: new Date(),
    userId: 0,
    user: { userId: 0, username: '' },
    campgroundId: 0,
    rating: 0
  };
  newRating: number = 0;
  campgroundId: number = 0;
  isLoggedIn: boolean = false;
  currentUserId: number = 0;

  constructor(
    private route: ActivatedRoute,
    private campgroundService: CampgroundService,
    private reviewService: ReviewService,
    public authService: AuthService,
    private flashMessageService: FlashMessageService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) { }

  ngOnInit(): void {
    this.currentUserId = this.authService.getUserId();
    console.log('Logged-in User ID:', this.currentUserId); // debugg

    this.randomImageUrl = `https://picsum.photos/400?random=${Math.random()}`;

    this.route.paramMap.subscribe((params) => {
      this.id = params.get('id');
      if (this.id) {
        this.loadCampgroundDetails(this.id);
        this.loadReviews(this.id);


        if (isPlatformBrowser(this.platformId)) {
          this.initializeMap();
        }

      }
      this.isLoggedIn = this.authService.isLoggedIn();
    });

    this.isLoggedIn = this.authService.isLoggedIn();
    this.currentUserId = this.authService.getUserId();
  }

  navigateToLogin(): void {
    this.flashMessageService.showMessage('Please log in to leave a review.', 5000);
    this.router.navigate(['/login']);
  }

  canAddOrDelete(): boolean {
    return this.authService.isAuthenticated();
  }

  submitReview(): void {
    if (!this.authService.isLoggedIn()) {
      this.flashMessageService.showMessage('Please log in to leave a review.', 5000);
      this.router.navigate(['/login']);
      return;
    }
  
    const reviewToSubmit: Review = {
      reviewId: 0,
      text: this.newReview.text,
      timestamp: new Date(),
      userId: this.authService.getUserId(),
      user: { userId: this.authService.getUserId(), username: this.authService.getUsername() || '' },
      campgroundId: this.campgroundId,
      rating: this.newRating,
    };
  
    this.reviewService.addReview(this.campgroundId, reviewToSubmit).subscribe({
      next: (review) => {
        console.log('Review successfully posted:', review);
  
        // Add the new review to the local reviews array
        this.reviews.push(review);
  
        this.newReview.text = '';
        this.newRating = 0;
        this.flashMessageService.showMessage('Review added successfully!', 5000);
      },
      error: (err) => {
        console.error('Error submitting review:', err);
        this.flashMessageService.showMessage(
          err.status === 401
            ? 'You must be logged in to submit a review.'
            : 'Failed to submit review. Please try again later.',
          5000
        );
      },
    });
  }
  
  
  loadCampgroundDetails(id: number): void {
    this.campgroundService.getCampground(id).subscribe({
      next: (data: Campground) => {
        this.campground = data;
        this.campgroundId = this.campground.campgroundId;
  
        if (!this.campground.images || this.campground.images.length === 0) {
          this.randomImageUrl = `https://picsum.photos/400?random=${Math.random()}`;
        }
  
        if (isPlatformBrowser(this.platformId)) {
          this.initializeMap();
        }
      },
      error: (err) => {
        console.error('Error fetching campground details', err);
      },
    });
  }
  
  

  loadReviews(campgroundId: number): void {
    this.reviewService.getReviewsForCampground(campgroundId).subscribe({
      next: (reviews: Review[]) => {
        console.log('Fetched reviews from API:', reviews); // Debug log
        // Ensure `reviews` is properly assigned
        this.reviews = Array.isArray(reviews) ? reviews : [];
      },
      error: (err) => {
        console.error('Error fetching reviews:', err);
        this.reviews = []; // Reset reviews to an empty array on error
      },
    });
  }
  
  
  deleteReview(reviewId: number): void {
    const review = this.reviews.find((r) => r.reviewId === reviewId);
  
    if (review && review.userId !== this.currentUserId) {
      this.flashMessageService.showMessage('You can only delete your own reviews!', 5000);
      return;
    }
  
    this.reviewService.deleteReview(reviewId).subscribe({
      next: () => {
        console.log('Review deleted successfully:', reviewId); // Debug log
        this.flashMessageService.showMessage('Review deleted successfully!', 5000);
        // Remove the deleted review from the UI
        this.reviews = this.reviews.filter((r) => r.reviewId !== reviewId);
      },
      error: (err) => {
        console.error('Error deleting review:', err);
        this.flashMessageService.showMessage('Cannot delete review!', 5000);
      },
    });
  }
  
  
  initializeMap(): void {
    if (!this.campground.latitude || !this.campground.longitude) {
      console.error('Campground does not have valid latitude and longitude values.');
      return;
    }
  
    const map = L.map('map').setView(
      [this.campground.latitude, this.campground.longitude],
      13
    );
  
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
    }).addTo(map);
  
    const customIcon = L.icon({
      iconUrl: this.campground.images.length > 0 
        ? this.campground.images[0].url 
        : 'assets/leaflet/marker-icon.png', // Default marker if no images
      shadowUrl: 'assets/leaflet/marker-shadow.png',
      iconSize: [40, 40], // Adjust size for better visuals
      iconAnchor: [20, 40], // Adjust anchor for better positioning
      popupAnchor: [0, -40], // Adjust popup positioning
      shadowSize: [50, 50], // Shadow size
    });
  
    // Add marker to the map
    L.marker([this.campground.latitude, this.campground.longitude], { icon: customIcon })
      .addTo(map)
      .bindPopup(`<b>${this.campground.name}</b><br>${this.campground.location}`);
  }
  
  
  deleteCampground(campgroundId: number): void {
    const token = this.authService.getToken();
  
    if (!token) {
      console.error('No token found. User is not authenticated.');
      return;
    }
  
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
  
    this.campgroundService.deleteCampground(campgroundId.toString(), headers).subscribe({
      next: () => {
        console.log('Campground deleted successfully!');
        this.router.navigate(['/campgrounds']);
      },
      error: (error) => {
        console.error('Error deleting campground:', error);
        alert('You are not authorized to delete this campground.');
      },
    });
  }
  
  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
    }
  }


}
