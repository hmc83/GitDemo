import { EventEmitter, Injectable } from '@angular/core';
import { City } from '../models/city';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CitiesService {

  cities: City[];
  upToDate: boolean = false;
  changedEmitter: EventEmitter<void> = new EventEmitter<void>(); 
  constructor(private http: HttpClient) {
    this.updateLocalCopy();
  }

  updateLocalCopy() {
    this.http.get<City[]>('https://localhost:7001/api/cities/1', { headers: { "Authorization": `Bearer ${localStorage['auth-token']}` } }).subscribe({
      next: (cities) => {
        this.cities = cities;
        this.upToDate = true;
        this.changedEmitter.emit();
      },
      error: (errResponse) => {
        this.cities = [];
        this.changedEmitter.emit();
        if (errResponse.error && errResponse.error.title && errResponse.error.status) {
          console.log('Error: ' + errResponse.error.title + ' ' + errResponse.error.status);
        }
        else {
          console.log(errResponse);
        }
      }
    });
  }

  public getCities(): City[] {
    return this.cities;
  }

  public postCity(city: City) {
    console.log(city);
    return this.http.post<City>('https://localhost:7001/api/cities/1', city, { headers: { "Authorization": `Bearer ${localStorage['auth-token']}` } }).pipe(tap(() => {
      this.upToDate = false;
      this.updateLocalCopy();
    }));
  }

  public putCity(city: City) {
    return this.http.put('https://localhost:7001/api/cities/1/' + city.cityID, city, { headers: { "Authorization": `Bearer ${localStorage['auth-token']}` } }).pipe(tap(() => {
      console.log('tap executed');
      this.upToDate = false;
      this.updateLocalCopy();
    }));
  }

  public deleteCity(city: City) {
    return this.http.delete('https://localhost:7001/api/cities/1/' + city.cityID, { headers: { "Authorization": `Bearer ${localStorage['auth-token']}` } }).pipe(tap(() => {
      console.log('tap executed');
      this.upToDate = false;
      this.updateLocalCopy();
    }));
  }
}
