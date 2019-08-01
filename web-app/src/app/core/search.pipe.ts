import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'search'
})
export class SearchPipe implements PipeTransform {

  transform(items: any[], term: string, ...properties: string[]): any {
    if (items && term) {
      return items.filter((x) => {
        try {
          for (const property of properties) {
            let pr = property.split('.');
            if (pr.length == 1) {
              return x[pr[0]].toLowerCase().search(term.toLowerCase()) > -1;
            } else if (pr.length == 2) {
              return x[pr[0]][pr[1]].toLowerCase().search(term.toLowerCase()) > -1;
            } else if (pr.length == 3) {
              return x[pr[0]][pr[1]][pr[2]].toLowerCase().search(term.toLowerCase()) > -1;
            }
          }
        } catch (error) {
          return false;
        }
      })
    } else {
      return items;
    }
  }

}
