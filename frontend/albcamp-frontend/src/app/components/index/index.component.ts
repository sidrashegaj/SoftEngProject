import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CampgroundService } from '../../services/campground.service';
import { FlashMessageService } from '../../services/flash-message.service';

@Component({
  selector: 'app-index',
  standalone: true,
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css'],
  imports: [CommonModule, FormsModule]
})
export class IndexComponent {
  searchQuery: string = '';
  campgrounds: any[] = [];
  noResultsFound: boolean = false;
  isNearbySearch: boolean = false;
  randomImageUrl: string = 'assets/default-image.jpg'; // Add this line


  constructor(private campgroundService: CampgroundService, private router: Router, private flashMessageService: FlashMessageService) { }
  findNearbyCampgrounds(): void {
    if (!navigator.geolocation) {
      alert('Geolocation is not supported by your browser.');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        const { latitude, longitude } = position.coords;
        console.log(`User's location: Latitude ${latitude}, Longitude ${longitude}`);

        // Call the service to fetch nearby campgrounds
        this.campgroundService.searchNearbyCampgrounds(latitude, longitude).subscribe({
          next: (results) => {
            this.campgrounds = results;
            this.noResultsFound = results.length === 0;
          },
          error: (err) => {
            console.error('Error finding nearby campgrounds:', err);
            this.noResultsFound = true;
          }
        });
      },
      (error) => {
        console.error('Error retrieving location:', error);
        alert('Unable to retrieve your location.');
      }
    );
  }

  // Navigate to campground details
  navigateToCampgrounds(campgroundId: number): void {
    this.router.navigate(['/campgrounds', campgroundId]);
  }
  navigateToAllCampgrounds(): void {
    this.router.navigate(['/campgrounds']);
  }
  
}
