import { ResolveFn } from "@angular/router";
import { City } from "../models/city";
import { CitiesService } from "../services/cities.service";
import { inject } from "@angular/core";
import { map, take } from "rxjs";

export const CitiesResolver: ResolveFn<City[]> = () => {
  const citiesService = inject(CitiesService);
  if (citiesService.upToDate) {
    return citiesService.getCities();
  }
  else {
    return citiesService.changedEmitter.pipe(
      take(1),
      map(() => {
        return citiesService.getCities();
      })
    )
  }
}
