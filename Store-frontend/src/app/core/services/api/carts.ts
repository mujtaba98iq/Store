import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AddCartItemBody, Cart, UpdateCartItemBody } from '@app/core/models/interfaces/cart';
import { ErrorNotification, errorNotification } from '@app/core/utils/error-notification';
import { environment } from '@env/environment';

const CARTS_URL = `${environment.apiBaseUrl}/carts`;

/**
 * The caller's own cart. The API addresses it by the token rather than by an id,
 * so a shopper can only ever reach their own - there is no cart id to guess.
 */
const MY_CART_URL = `${CARTS_URL}/me`;
const MY_CART_ITEMS_URL = `${MY_CART_URL}/items`;

/**
 * One method per endpoint of the API's cart resource. Every one of them is behind
 * the `User,Admin` roles, so nothing here is reached while signed out - `CartStore`
 * is what holds that rule.
 *
 * They all answer with the whole cart, lines included, which is why none of these
 * return anything narrower.
 */
@Injectable({ providedIn: 'root' })
export class ApiCartsService {
  private readonly http = inject(HttpClient);

  /**
   * The cart is created on first use, so a shopper who has never shopped gets an
   * empty one back rather than a 404.
   *
   * Silent: this runs on sign-in without anyone asking for it, and the cart page
   * reports its own failure with somewhere to go next.
   */
  mine(): Observable<Cart> {
    return this.http.get<Cart>(MY_CART_URL, {
      context: errorNotification(ErrorNotification.Silent),
    });
  }

  addItem(body: AddCartItemBody): Observable<Cart> {
    return this.http.post<Cart>(MY_CART_ITEMS_URL, body);
  }

  /** Sets the line's quantity outright; the API refuses zero. */
  updateItem(cartItemId: string, body: UpdateCartItemBody): Observable<Cart> {
    return this.http.patch<Cart>(`${MY_CART_ITEMS_URL}/${cartItemId}`, body);
  }

  removeItem(cartItemId: string): Observable<Cart> {
    return this.http.delete<Cart>(`${MY_CART_ITEMS_URL}/${cartItemId}`);
  }

  /** Empties every line at once, leaving the cart itself in place. */
  clear(): Observable<Cart> {
    return this.http.delete<Cart>(MY_CART_ITEMS_URL);
  }
}
