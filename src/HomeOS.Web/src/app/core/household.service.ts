import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface Household {
  id: string;
  name: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class HouseholdService {
  private http = inject(HttpClient);
  private baseUrl = '/api/households';

  private activeHouseholdSubject = new BehaviorSubject<Household | null>(null);
  activeHousehold$ = this.activeHouseholdSubject.asObservable();

  get activeHouseholdId(): string | null {
    return this.activeHouseholdSubject.value?.id || localStorage.getItem('active_household_id');
  }

  setActiveHousehold(household: Household) {
    this.activeHouseholdSubject.next(household);
    localStorage.setItem('active_household_id', household.id);
  }

  getMyHouseholds(): Observable<Household[]> {
    return this.http.get<Household[]>(this.baseUrl).pipe(
      tap(households => {
        if (households.length > 0) {
          const savedId = localStorage.getItem('active_household_id');
          const toSelect = households.find(h => h.id === savedId) || households[0];
          this.setActiveHousehold(toSelect);
        } else {
          this.activeHouseholdSubject.next(null);
          localStorage.removeItem('active_household_id');
        }
      })
    );
  }

  createHousehold(name: string): Observable<Household> {
    return this.http.post<Household>(this.baseUrl, { name });
  }

  generateInvite(householdId: string): Observable<{ code: string, expiresAt: string }> {
    return this.http.post<{ code: string, expiresAt: string }>(`${this.baseUrl}/${householdId}/invite`, {});
  }

  joinHousehold(code: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/join`, { code });
  }
}
