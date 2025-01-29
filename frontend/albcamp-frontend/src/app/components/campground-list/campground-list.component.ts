import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Campground } from '../../models/campground.model';
import { CampgroundService } from '../../services/campground.service';
import * as mapboxgl from 'mapbox-gl';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-campground-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './campground-list.component.html',
  styleUrls: ['./campground-list.component.css'],
  providers: [CampgroundService],
})
export class CampgroundListComponent implements OnInit {
  campgrounds: Campground[] = [];
  map!: mapboxgl.Map;
  location: string = ''; // Holds the location filter from the search bar

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private campgroundService: CampgroundService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.initializeMap();
      this.loadCampgrounds(); // Load all campgrounds initially
    }
  }

  loadCampgrounds(): void {
    this.campgroundService.getCampgrounds().subscribe({
      next: (response: any) => {
        // Extract the actual campgrounds array from the `$values` property
        if (response && response.$values) {
          this.campgrounds = response.$values.map((campground: any) => {
            if (campground.images && campground.images.$values) {
              campground.images = campground.images.$values;
            } else {
              campground.images = [
                { url: this.getRandomImageUrl(), filename: 'random-placeholder' },
              ];
            }
            return campground;
          });
          this.addMarkersToMap(this.campgrounds); // Add markers to the map
        }
      },
      error: (error: any) => {
        console.error('Error fetching campgrounds', error);
      },
    });
  }
  

  // Initialize the Mapbox map
  initializeMap(): void {
    if (isPlatformBrowser(this.platformId)) {
      mapboxgl.default.accessToken = environment.mapbox.accessToken;
      this.map = new mapboxgl.Map({
        container: 'map',
        style: 'mapbox://styles/mapbox/streets-v11',
        center: [19.8189, 41.3275], // Default center on Albania
        zoom: 6,
      });
      this.map.addControl(new mapboxgl.NavigationControl());
    }
  }

  // Generate a random placeholder image URL
  getRandomImageUrl(): string {
    return `https://picsum.photos/400?random=${Math.random()}`;
  }

  // Navigate to the login page
  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  // Add markers for campgrounds to the map
  addMarkersToMap(campgrounds: Campground[]): void {
    if (this.map) {
      campgrounds.forEach((campground) => {
        if (campground.geometry && campground.geometry.coordinates) {
          const [longitude, latitude] = campground.geometry.coordinates;

          new mapboxgl.Marker()
            .setLngLat([longitude, latitude]) // Place marker at the campground's coordinates
            .setPopup(
              new mapboxgl.Popup({ offset: 25 }).setHTML(
                `<h3>${campground.name}</h3><p>${campground.location}</p>`
              )
            )
            .addTo(this.map); // Add marker to the map
        }
      });
    }
  }

  // Handle search functionality
  search(): void {
    this.loadCampgrounds(); // Reload campgrounds with the location filter
  }
}
