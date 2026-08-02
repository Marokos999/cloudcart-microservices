import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  imports: [],
  templateUrl: './footer.html',
  styles: [`
    .footer {
      background: #080808;
      border-top: 1px solid rgba(255,255,255,0.07);
      margin-top: 80px;
    }

    .footer-container {
      max-width: 1320px;
      margin: 0 auto;
      padding: 64px 32px;
      display: flex;
      justify-content: space-between;
      gap: 48px;
    }

    .footer-brand {
      max-width: 280px;

      .logo {
        font-family: 'Bebas Neue', sans-serif;
        font-size: 28px;
        letter-spacing: 3px;
        color: #fff;
        margin-bottom: 16px;
        display: block;
        span { color: #cc0000; }
      }

      p {
        color: #666;
        font-size: 13px;
        line-height: 1.7;
      }
    }

    .footer-links {
      display: flex;
      gap: 64px;
    }

    .link-group {
      display: flex;
      flex-direction: column;
      gap: 12px;

      h4 {
        font-size: 10px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 3px;
        color: #fff;
        margin-bottom: 4px;
      }

      a {
        font-size: 13px;
        color: #666;
        text-decoration: none;
        transition: color 0.2s;

        &:hover { color: #cc0000; }
      }
    }

    .footer-bottom {
      max-width: 1320px;
      margin: 0 auto;
      padding: 20px 32px;
      border-top: 1px solid rgba(255,255,255,0.07);
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 11px;
      color: #444;
      text-transform: uppercase;
      letter-spacing: 1px;

      .socials {
        display: flex;
        gap: 24px;

        a {
          color: #444;
          text-decoration: none;
          transition: color 0.2s;
          &:hover { color: #cc0000; }
        }
      }
    }
  `]
})
export class Footer {}
