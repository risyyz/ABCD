import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

declare const grecaptcha: {
  enterprise: {
    ready: (callback: () => void) => void;
    execute: (siteKey: string, options: { action: string }) => Promise<string>;
  };
};

@Injectable({
  providedIn: 'root'
})
export class RecaptchaService {
  private siteKey = environment.recaptcha.siteKey;
  private scriptLoaded = false;

  constructor() {
    if (this.isConfigured) {
      this.loadScript();
    }
  }

  private get isConfigured(): boolean {
    return !!this.siteKey && !this.siteKey.startsWith('YOUR_');
  }

  execute(action: string): Observable<string> {
    if (!this.isConfigured) {
      return new Observable<string>(observer => {
        observer.next('');
        observer.complete();
      });
    }

    return new Observable<string>(observer => {
      this.ensureScriptLoaded().then(() => {
        grecaptcha.enterprise.ready(() => {
          grecaptcha.enterprise.execute(this.siteKey, { action }).then(
            token => {
              observer.next(token);
              observer.complete();
            },
            error => observer.error(error)
          );
        });
      }).catch(error => observer.error(error));
    });
  }

  private ensureScriptLoaded(): Promise<void> {
    if (this.scriptLoaded) {
      return Promise.resolve();
    }
    return this.loadScript();
  }

  private loadScript(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.scriptLoaded) {
        resolve();
        return;
      }

      const existingScript = document.querySelector(`script[src*="recaptcha/enterprise.js"]`);
      if (existingScript) {
        this.scriptLoaded = true;
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = `https://www.google.com/recaptcha/enterprise.js?render=${this.siteKey}`;
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.scriptLoaded = true;
        resolve();
      };
      script.onerror = () => reject(new Error('Failed to load reCAPTCHA script'));
      document.head.appendChild(script);
    });
  }
}
