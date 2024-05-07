import { Component } from '@angular/core';
import { CitiesService } from '../services/cities.service';
import { City } from '../models/city';
import { ActivatedRoute } from '@angular/router';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrl: './cities.component.css'
})
export class CitiesComponent {

  cities: City[];
  cityForm: FormGroup;
  editCityID: string | null = null;

  constructor(private citiesService: CitiesService, private accountService: AccountService, private activatedRoute: ActivatedRoute) { }

  ngOnInit() {
    this.cities = this.activatedRoute.snapshot.data.cities;
    this.citiesService.changedEmitter.subscribe({
      next: () => {
        this.cities = this.citiesService.getCities();
      }
    });

    this.cityForm = new FormGroup({
      'cityName': new FormControl(null, [Validators.required])
    })
  }

  onSubmit() {
    console.log(this.cityForm.value);
    this.citiesService.postCity(this.cityForm.value).
      subscribe({
        next: (newlyCreatedCity) => {
          console.log('The newly created city is: ');
          console.log(newlyCreatedCity);
          this.cityForm.reset();
        },
        error: (errResponse) => {
          if (errResponse.error.detail) {
            console.log('Error: ' + errResponse.error.detail);
          }
          else {
            console.log(errResponse);
          }
        }
      });
  }

  editClicked(city : City) {
    this.editCityID = city.cityID;
  }

  updateClicked(city: City) {
    this.citiesService.putCity(city).subscribe({
      next: () => {
        this.editCityID = null;
      },
      error: (errResponse) => {
        if (errResponse.error.detail) {
          console.log('Error: ' + errResponse.error.detail);
        }
        else {
          console.log(errResponse);
        }
      }
    });
  }

  deleteClicked(city: City) {
    if (confirm("Are you sure to delete the city?")) {
      this.citiesService.deleteCity(city).subscribe({
        next: () => {
        },
        error: (errResponse) => {
          if (errResponse.error.detail) {
            console.log('Error: ' + errResponse.error.detail);
          }
          else {
            console.log(errResponse);
          }
        }
      });
    }
  }

  refreshClicked() {
    this.accountService.postGenerateNewToken().subscribe({
      next: () => {
        this.citiesService.updateLocalCopy();
      },
      error: (errResponse) => {
        console.log(errResponse.error);
      }
    });
  }
}
