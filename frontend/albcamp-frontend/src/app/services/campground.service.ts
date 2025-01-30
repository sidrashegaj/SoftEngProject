import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Campground } from '../models/campground.model';

@Injectable({
  providedIn: 'root',
})
export class CampgroundService {
  private readonly apiUrl = 'http://localhost:5259/api/Campgrounds';

  constructor(private http: HttpClient) { }

  getCampgrounds(location?: string): Observable<Campground[]> {
    let params = new HttpParams();
    if (location) {
      params = params.set('location', location);
    }
    return this.http.get<Campground[]>(this.apiUrl, { params });
  }
  getCampground(id: number): Observable<Campground> {
    return this.http.get<Campground>(`${this.apiUrl}/${id}`);
  }
  deleteCampground(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
  searchCampgrounds(query: string): Observable<Campground[]> {
    const url = `https://localhost:5259/api/campgrounds/search?q=${query}`;
    return this.http.get<Campground[]>(url).pipe(
      catchError(this.handleError) // Catch errors and handle them
    );
  }
  updateCampground(id: number, formData: FormData): Observable<Campground> {

    return this.http.put<Campground>(`${this.apiUrl}/${id}`, formData);

  }

  addCampground(formData: FormData): Observable<any> {

    return this.http.post<any>(`${this.apiUrl}`, formData);

  }



  private handleError(error: any): Observable<never> {
    console.error('An error occurred:', error);
    return throwError(() => new Error('Something bad happened; please try again later.'));
  }
}