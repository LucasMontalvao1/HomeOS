import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-mesh min-h-screen flex items-center justify-center p-4">
      <div class="glass-card w-full max-w-md p-8">
        
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold text-emerald-600 mb-2">HomeOS</h1>
          <p class="text-slate-500">Acesse sua casa inteligente</p>
        </div>

        <div *ngIf="errorMessage" class="bg-red-50 border border-red-200 text-red-600 p-3 rounded-xl text-sm mb-4">
          {{ errorMessage }}
        </div>

        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">E-mail</label>
            <input 
              type="text"
              name="email"
              [(ngModel)]="email"
              class="input-glass"
              placeholder="seu@email.com"
              (keyup.enter)="onSubmit()"
              (input)="errorMessage = ''"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Senha</label>
            <input 
              type="password"
              name="password"
              [(ngModel)]="password"
              class="input-glass"
              placeholder="••••••••"
              (keyup.enter)="onSubmit()"
              (input)="errorMessage = ''"
            />
          </div>

          <button 
            type="button"
            (click)="onSubmit()"
            [disabled]="isLoading"
            class="btn-primary w-full flex justify-center items-center h-12 mt-2"
          >
            <span *ngIf="!isLoading">Entrar</span>
            <span *ngIf="isLoading" class="animate-pulse">Acessando...</span>
          </button>

          <div class="relative flex items-center py-1">
            <div class="flex-grow border-t border-slate-200"></div>
            <span class="flex-shrink-0 mx-4 text-slate-400 text-sm">ou</span>
            <div class="flex-grow border-t border-slate-200"></div>
          </div>

          <button 
            type="button" 
            disabled
            class="w-full flex justify-center items-center h-12 bg-white text-slate-700 font-medium py-3 px-6 rounded-2xl border border-slate-200 shadow-sm transition-all duration-300 opacity-60 cursor-not-allowed"
          >
            <svg class="w-5 h-5 mr-3" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
              <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
              <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/>
              <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
            </svg>
            Entrar com Google (Em breve)
          </button>
        </div>

        <div class="text-center mt-6">
          <button 
            type="button" 
            (click)="goToRegister()"
            class="text-sm text-emerald-600 font-medium hover:text-emerald-700 hover:underline"
          >
            Ainda não tem conta? Criar agora
          </button>
        </div>

      </div>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  private authService = inject(AuthService);
  private router = inject(Router);

  onSubmit() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Preencha o e-mail e a senha.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.error || 'E-mail ou senha incorretos. Tente novamente.';
      }
    });
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }
}
