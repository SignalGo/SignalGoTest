import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SignalgoService {

  constructor() { }

  deserializeReferences<T>(obj: any, ids: {} = {}, mappedObjects: Array<T> = []): any {
    var type = typeof obj;
    if (type == "string" || type == "number" || obj === Date || obj == null || obj == undefined) {
      return obj;
    }
    if (mappedObjects.indexOf(obj) >= 0 && !obj.$values) {
      return obj;
    }
    mappedObjects.push(obj);
    if (obj.$id) {
      if (obj.$values)
        ids[obj.$id] = obj.$values;
      else
        ids[obj.$id] = obj;
      delete obj.$id;
    }
    if (obj.$ref) {
      var ref = obj.$ref;
      obj = ids[obj.$ref];
      return obj;
    }
    if (obj instanceof Array) {
      var newArray = [];
      obj.forEach(x => {
        if (x.$ref) {
          x = ids[x.$ref];
          if (x) {
            newArray.push(x);
            delete x.$ref;
          }
        }
        else {
          newArray.push(this.deserializeReferences(x, ids, mappedObjects));
        }
      });
      return newArray;
    }
    else if (obj.$values) {
      var newArray = [];
      obj.$values.forEach(x => {
        if (x.$ref) {
          x = ids[x.$ref];
          if (x) {
            newArray.push(x);
            delete x.$ref;
          }
        }
        else {
          newArray.push(this.deserializeReferences(x, ids, mappedObjects));
        }
      });
      delete obj.$values;
      return newArray;
    }
    else {
      var properties = Object.getOwnPropertyNames(obj);
      for (var i = 0; i < properties.length; i++) {
        obj[properties[i]] = this.deserializeReferences(obj[properties[i]], ids, mappedObjects);
      }
    }

    return obj;
  }
  serializeReferences<T>(obj: any, isMainObject: boolean = true, mappedObjects: any = []): any {
    if (obj == null || obj == undefined)
      return obj;
    var type = typeof obj;
    if (type != "object" || obj instanceof Date)
      return obj;
    // else if (obj instanceof moment || obj._isAMomentObject) {
    //   return new Date(obj);
    // }
    var newObj = null;
    newObj = {};
    let find = mappedObjects.indexOf(obj);
    if (!isMainObject) {
      let currentId = mappedObjects.length + 1;
      newObj["$id"] = currentId;
      mappedObjects.push(obj);
    }
    if (obj instanceof Array) {
      newObj.$values = [];
      if (find != -1) {
        newObj.$values = { $ref: find + 1 };
      }
      else {
        obj.forEach(x => {
          let find = mappedObjects.indexOf(x);
          if (find != -1) {
            newObj.$values.push({ $ref: find + 1 });
          }
          else {
            if (isMainObject)
              newObj.$values.push(this.serializeReferences(x, false, []));
            else
              newObj.$values.push(this.serializeReferences(x, false, mappedObjects));
          }
        });
      }
    }
    else {
      var properties = Object.getOwnPropertyNames(obj);
      for (var i = 0; i < properties.length; i++) {
        let propertyValue = obj[properties[i]];
        let find = mappedObjects.indexOf(propertyValue);
        newObj[properties[i]] = undefined;
        if (find != -1) {
          newObj[properties[i]] = { $ref: find + 1 };
        }
        else {
          if (isMainObject)
            newObj[properties[i]] = this.serializeReferences(propertyValue, false, []);
          else
            newObj[properties[i]] = this.serializeReferences(propertyValue, false, mappedObjects);
        }
      }
    }
    return newObj;
  }
}
