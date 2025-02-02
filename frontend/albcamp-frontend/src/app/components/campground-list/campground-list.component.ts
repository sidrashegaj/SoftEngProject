import {
  Component,
  Inject,
  OnInit,
  PLATFORM_ID,
  AfterViewInit,
} from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Campground } from '../../models/campground.model';
import { CampgroundService } from '../../services/campground.service';
import * as L from 'leaflet';

@Component({
  selector: 'app-campground-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './campground-list.component.html',
  styleUrls: ['./campground-list.component.css'],
  providers: [CampgroundService],
})
export class CampgroundListComponent implements OnInit, AfterViewInit {
  campgrounds: Campground[] = [];
  map!: L.Map;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private campgroundService: CampgroundService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCampgrounds();
  }

  ngAfterViewInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.initializeMap();
    }
  }

  initializeMap(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.map = L.map('map').setView([41.3275, 19.8189], 7); // Centered on Albania

      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution:
          'Map data © <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      }).addTo(this.map);
    }
  }

  loadCampgrounds(): void {
    this.campgroundService.getCampgrounds().subscribe({
      next: (response: any) => {
        if (response && response.$values) {
          this.campgrounds = response.$values.map((campground: Campground) => {
            if (!campground.geometry || !campground.geometry.coordinates) {
              campground.geometry = { coordinates: [0, 0] }; // Default coordinates
            }

            if (!campground.images || campground.images.length === 0) {
              campground.images = [
                { url: this.getRandomImageUrl(), filename: 'placeholder' },
              ];
            }

            return campground;
          });

          this.addMarkersToMap(this.campgrounds);
        }
      },
      error: (err) => {
        console.error('Error fetching campgrounds', err);
      },
    });
  }

  addMarkersToMap(campgrounds: Campground[]): void {
    if (this.map) {
      campgrounds.forEach((campground) => {
        const [longitude, latitude] = campground.geometry.coordinates;

        if (longitude && latitude) {
          L.marker([latitude, longitude])
            .addTo(this.map)
            .bindPopup(
              `<b>${campground.name}</b><br>${campground.location}`
            );
        }
      });
    }
  }

  getRandomImageUrl(): string {
    return `https://picsum.photos/400?random=${Math.random()}`;
  }
}
