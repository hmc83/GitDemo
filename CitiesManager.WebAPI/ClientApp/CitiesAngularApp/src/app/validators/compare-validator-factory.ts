import { FormControl, FormGroup, ValidatorFn } from "@angular/forms";

export function compareValidatorFactory(controlToValidateName: string, controlToCompareName: string): ValidatorFn {
  return (formGrp: FormGroup) => {
    const controlToValidate: FormControl = formGrp.get(controlToValidateName) as FormControl;
    const controlToCompare: FormControl = formGrp.get(controlToCompareName) as FormControl;

    if (controlToValidate.value !== controlToCompare.value) {
      if (formGrp.get(controlToCompareName).errors === null) {
        formGrp.get(controlToCompareName).setErrors({ 'match': true });
      }
      else {
        formGrp.get(controlToCompareName).errors['match'] = true;
      }
      return null;
    }
  }
}
