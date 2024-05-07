import { HttpClient } from "@angular/common/http";
import { inject } from "@angular/core";
import { AbstractControl, AsyncValidatorFn, FormControl, ValidationErrors } from "@angular/forms";
import { Observable, catchError, map, of } from "rxjs";

export function existingEmailValidatorFactory(http: HttpClient): AsyncValidatorFn {
  return (emailControl: FormControl): Observable<ValidationErrors | null> => {
    return http.get("https://localhost:7001/api/account/1/isemailalreadyregistered?email=" + emailControl.value)
      .pipe(
        map(result => {
          return result ? { emailTaken: true } : null;
        }),
        catchError(() => {
          return of(null);
        })
      );
  };
}

