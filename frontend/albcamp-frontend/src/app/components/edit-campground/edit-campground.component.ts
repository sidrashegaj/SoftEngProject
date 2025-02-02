import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms'; 
import { CommonModule } from '@angular/common';
import { CampgroundService } from '../../services/campground.service';
import { Campground } from '../../models/campground.model';
import { FlashMessageService } from '../../services/flash-message.service';

@Component({
  selector: 'app-edit-campground',
  standalone: true,
  templateUrl: './edit-campground.component.html',
  styleUrls: ['./edit-campground.component.css'],
  imports: [CommonModule, FormsModule, RouterModule], 
})
export class EditCampgroundComponent implements OnInit {
  campgroundId!: number;
  campground: Campground = {
    title: '',
    location: '',
    price: 0,
    description: '',
    images: [],
    name: '',
    longitude: 0, // Added missing properties
    latitude: 0,  // Added missing properties
    campgroundId: 0,
    author: {
      _id: 0,
      username: '',
    },
    geometry: {
      coordinates: [0, 0],
    },
  };
  deleteImages: string[] = [];
  selectedImages: File[] = [];

  constructor(
    private route: ActivatedRoute,
    private campgroundService: CampgroundService,
    private router: Router,
    private flashMessageService: FlashMessageService
  ) {}

  ngOnInit(): void {
    this.campgroundId = +this.route.snapshot.paramMap.get('id')!;
    this.loadCampgroundDetails();
  }
  loadCampgroundDetails(): void {
    this.campgroundService.getCampground(this.campgroundId).subscribe((campground) => {
      this.campground = campground;
  
      // Ensure `images` is an array
      if (!Array.isArray(this.campground.images)) {
        this.campground.images = [];
      }
  
      this.deleteImages = this.campground.images.map((img) => img.filename);
    });
  }
  
  onImageSelected(event: any): void {
    this.selectedImages = Array.from(event.target.files);
  }
  onSubmit(): void {
    const formData = new FormData();
  
    // Append basic details
    formData.append('name', this.campground.name);
    formData.append('location', this.campground.location);
    formData.append('price', this.campground.price.toString());
    formData.append('description', this.campground.description);
    formData.append('latitude', this.campground.latitude.toString());
    formData.append('longitude', this.campground.longitude.toString());
  
    // Append new images
    for (const file of this.selectedImages) {
      formData.append('images', file);
    }
  
    // Append existing images to keep
    for (const img of this.campground.images) {
      if (!this.deleteImages.includes(img.filename)) {
        formData.append('existingImages[]', img.filename);
      }
    }
  
    // Append images to delete
    for (const filename of this.deleteImages) {
      formData.append('deleteImages[]', filename);
    }
  
    this.campgroundService.updateCampground(this.campgroundId, formData).subscribe({
      next: () => {
        this.flashMessageService.showMessage('Campground updated successfully!', 5000);
        this.router.navigate(['/campgrounds']);
      },
      error: (err) => {
        console.error('Error updating campground', err);
        this.flashMessageService.showMessage('Failed to update the campground.', 5000);
      },
    });
  }
  
  
}
