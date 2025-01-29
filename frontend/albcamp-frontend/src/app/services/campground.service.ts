import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Campground } from '../models/campground.model';

@Injectable({
  providedIn: 'root',
})
export class CampgroundService {
  private readonly apiUrl = 'http://localhost:5259/api/Campgrounds';

  constructor(private http: HttpClient) {}

  getCampgrounds(location?: string): Observable<Campground[]> {
    let params = new HttpParams();
    if (location) {
      params = params.set('location', location);
    }
    return this.http.get<Campground[]>(this.apiUrl, { params });
  }
}
