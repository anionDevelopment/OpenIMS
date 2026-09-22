import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'timestamp', standalone: false })
export class TimestampPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return '';
    }
    return value.replace('T', ' ').replace(/([+-]\d{2}:\d{2})$/, ' $1');
  }
}
