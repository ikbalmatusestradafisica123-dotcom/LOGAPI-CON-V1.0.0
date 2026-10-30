/// <reference types="google.maps" />
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';


declare global {
  interface Window {
    google?: typeof google;
  }
}

@Injectable({
  providedIn: 'root',
})
export class GoogleMapsLoader {
  private readonly apiKey =
    environment.googleMapsApiKey;

  private loadingPromise: Promise<void> | null = null;

  load(): Promise<void> {
    if (window.google?.maps) {
      return Promise.resolve();
    }

    if (this.loadingPromise) {
      return this.loadingPromise;
    }

    this.loadingPromise = new Promise<void>(
      (resolve, reject) => {
        const existingScript =
          document.querySelector(
            'script[data-google-maps]',
          );

        if (existingScript) {
          existingScript.addEventListener(
            'load',
            () => resolve(),
          );

          existingScript.addEventListener(
            'error',
            () => reject(
              new Error(
                'No se pudo cargar Google Maps.',
              ),
            ),
          );

          return;
        }

        const script =
          document.createElement('script');

        script.dataset['googleMaps'] = 'true';
        script.async = true;
        script.defer = true;
        script.src =
          'https://maps.googleapis.com/maps/api/js' +
          `?key=${this.apiKey}` +
          '&v=weekly';

        script.onload = () => resolve();

        script.onerror = () => reject(
          new Error(
            'No se pudo cargar Google Maps.',
          ),
        );

        document.head.appendChild(script);
      },
    );

    return this.loadingPromise;
  }
}
