import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5259/api/auth'; // Backend API URL
  private currentUserSubject: BehaviorSubject<any>;
  public currentUser: Observable<any>;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: any // Determines if code runs in browser or SSR
  ) {
    const storedUser = isPlatformBrowser(platformId)
      ? localStorage.getItem('currentUser')
      : null;

    this.currentUserSubject = new BehaviorSubject<any>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue() {
    return this.currentUserSubject.value;
  }

  /**
   * Login user and store their token and details in localStorage.
   */
  login(username: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, { username, password }).pipe(
      map((response) => {
        if (response && response.token) {
          try {
            const decodedToken: any = jwtDecode(response.token);
            console.log('Decoded token:', decodedToken);

            const user = {
              userId: decodedToken.UserId || decodedToken.userId || decodedToken.id,
              username: decodedToken.sub,
              token: response.token,
            };

            // Store in localStorage
            localStorage.setItem('currentUser', JSON.stringify(user));
            this.currentUserSubject.next(user);

            return user;
          } catch (error) {
            console.error('Invalid token:', error);
            throw new Error('Invalid token received from the server');
          }
        }
        throw new Error('Token not received');
      })
    );
  }

  /**
   * Register user and store their token and details in localStorage.
   */
  register(username: string, email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, { username, email, password }).pipe(
      map((response) => {
        if (response && response.token) {
          try {
            const decodedToken: any = jwtDecode(response.token);

            const user = {
              userId: decodedToken.UserId || decodedToken.userId || decodedToken.id,
              username: decodedToken.sub,
              token: response.token,
            };

            // Store in localStorage
            localStorage.setItem('currentUser', JSON.stringify(user));
            this.currentUserSubject.next(user);

            return user;
          } catch (error) {
            console.error('Invalid token:', error);
            throw new Error('Invalid token received from the server');
          }
        }
        throw new Error('Token not received');
      })
    );
  }

  /**
   * Logs out the user, clears localStorage and BehaviorSubject.
   */
  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('currentUser');
      this.currentUserSubject.next(null);
    }
  }

  /**
   * Checks if the user is authenticated (token exists and is not expired).
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        const now = Math.floor(Date.now() / 1000); // Current time in seconds
        if (decodedToken.exp && decodedToken.exp < now) {
          console.warn('Token has expired');
          this.logout();
          return false;
        }
        return true;
      } catch (error) {
        console.error('Error decoding token:', error);
        return false;
      }
    }
    return false;
  }

  /**
   * Retrieves the token of the current user.
   */
  getToken(): string | null {
    const currentUser = this.currentUserValue;
    return currentUser ? currentUser.token : null;
  }

  /**
   * Checks if a user is logged in (checks localStorage for user data).
   */
  isLoggedIn(): boolean {
    return !!localStorage.getItem('currentUser');
  }

  /**
   * Retrieves the userId of the current user from localStorage.
   */
  getUserId(): number {
    const storedUser = localStorage.getItem('currentUser');
    const user = storedUser ? JSON.parse(storedUser) : null;
    return user ? user.userId : 0;
  }

  /**
   * Retrieves the username of the current user from localStorage.
   */
  getUsername(): string | null {
    const currentUser = this.currentUserValue;
    return currentUser ? currentUser.username : null;
  }
}
