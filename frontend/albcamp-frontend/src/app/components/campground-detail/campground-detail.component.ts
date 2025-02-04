import { Component, OnDestroy, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { Campground } from '../../models/campground.model';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { CampgroundService } from '../../services/campground.service';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import * as mapboxgl from 'mapbox-gl';
import { Review } from '../../models/review.model';
import { ReviewService } from '../../services/review.service';
import { AuthService } from '../../services/auth.service';
import { HttpHeaders } from '@angular/common/http';
import { FlashMessageService } from '../../services/flash-message.service';
import { MapboxService } from '../../services/mapbox.service';


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
    private mapboxService: MapboxService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.campgroundId = Number(params.get('id')); 
      console.log('Campground ID:', this.campgroundId);

      if (this.campgroundId) {
        this.loadCampgroundDetails(this.campgroundId);
        this.loadReviews(this.campgroundId);
      }

      if (isPlatformBrowser(this.platformId)) {
        this.initializeMap();
      }

      this.isLoggedIn = this.authService.isLoggedIn();
      this.currentUserId = this.authService.getUserId();
      console.log('Current User ID:', this.currentUserId); 
    });
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

    console.log('Submitting Review:', reviewToSubmit); // Debug log

    this.reviewService.addReview(this.campgroundId, reviewToSubmit).subscribe({
      next: (review) => {
        console.log('Review successfully posted:', review);

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
      next: (data: any) => {
        console.log('Raw Campground Data in Detail:', data);

        this.campground = {
          ...data,
          reviews: data.reviews?.$values || [],
          images: data.images?.$values?.sort((a: any, b: any) => {
            return a.imageId - b.imageId || a.filename.localeCompare(b.filename);
          }) || [],
        };
        this.reviews = [...(this.campground.reviews || [])]; 
        console.log('Processed Campground Data in Detail:', this.campground);

        if (isPlatformBrowser(this.platformId)) {
          this.initializeMap();
        }
      },
      error: (err) => {
        console.error('Error fetching campground details:', err);
      },
    });
  }

  loadReviews(campgroundId: number): void {
    this.reviewService.getReviewsForCampground(campgroundId).subscribe({
      next: (reviews: any) => {
        if (reviews && Array.isArray(reviews)) {
          this.reviews = reviews;
        } else if (reviews?.$values) {
          this.reviews = reviews.$values;
        } else {
          this.reviews = [];
        }
        console.log('Processed Reviews:', this.reviews);
      },
      error: (err) => {
        console.error('Error fetching reviews:', err);
        this.reviews = [];
      },
    });
  }

  deleteReview(reviewId: number): void {
    const review = this.reviews.find((r) => r.reviewId === reviewId);
  
    if (review) {
      console.log('Review User ID:', review.userId); 
      console.log('Current User ID:', this.currentUserId); 
  
      if (Number(review.userId) === this.currentUserId) { 
        this.reviewService.deleteReview(reviewId).subscribe({
          next: () => {
            console.log('Review deleted successfully:', reviewId);
            this.flashMessageService.showMessage('Review deleted successfully!', 5000);
  
            this.reviews = this.reviews.filter((r) => r.reviewId !== reviewId);
          },
          error: (err) => {
            console.error('Error deleting review:', err);
            this.flashMessageService.showMessage('An error occurred while deleting the review.', 5000);
          },
        });
      } else {
        this.flashMessageService.showMessage(
          'You are not authorized to delete this review.',
          5000
        );
      }
    } else {
      console.warn('Review not found.');
      this.flashMessageService.showMessage('Review not found.', 5000);
    }
  }
  
  

  initializeMap(): void {
    if (!this.campground.latitude || !this.campground.longitude) {
      console.error('Campground does not have valid latitude and longitude values.');
      return;
    }

    this.map = this.mapboxService.initializeMap(
      'map',
      [this.campground.longitude, this.campground.latitude],
      13
    );

    this.map.addControl(new mapboxgl.NavigationControl());

    const marker = new mapboxgl.Marker({ color: '#FF5733' })
      .setLngLat([this.campground.longitude, this.campground.latitude])
      .setPopup(
        new mapboxgl.Popup({ offset: 25 }).setHTML(
          `<b>${this.campground.name}</b><br>${this.campground.location}`
        )
      )
      .addTo(this.map);
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
        this.flashMessageService.showMessage('You are not authorized to delete this campground.', 5000);
      },
    });
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
    }
  }


}
