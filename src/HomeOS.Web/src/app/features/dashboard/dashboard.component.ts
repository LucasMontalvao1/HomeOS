import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { HouseholdService, Household } from '../../core/household.service';
import { ShoppingListService } from '../../core/shopping-list.service';
import { ProductService } from '../../core/product.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-mesh min-h-screen p-6">
      <nav class="glass-card mb-8 p-4 flex justify-between items-center">
        <div class="flex items-center gap-4">
          <h1 class="text-xl font-bold text-emerald-600">HomeOS</h1>
          
          <div class="hidden md:flex items-center gap-2 border-l border-slate-200 pl-4 ml-2">
            <span class="text-sm text-slate-500">Casa Atual:</span>
            @if (households.length > 0) {
              <select
                [ngModel]="activeHousehold?.id"
                (ngModelChange)="changeActiveHousehold($event)"
                class="bg-transparent text-sm font-semibold text-slate-700 focus:outline-none cursor-pointer">
                @for (h of households; track h.id) {
                  <option [value]="h.id">{{ h.name }}</option>
                }
              </select>
            }
          </div>
        </div>
        
        <div class="flex items-center gap-4">
          <span class="text-slate-600 font-medium hidden md:inline">Olá, {{ user()?.name }}</span>
          <button (click)="logout()" class="btn-primary py-2 px-4 text-sm">Sair</button>
        </div>
      </nav>

      <!-- Loading State -->
      @if (isLoading) {
        <div class="flex flex-col items-center justify-center mt-24 gap-4">
          <div class="w-10 h-10 border-4 border-emerald-400 border-t-transparent rounded-full animate-spin"></div>
          <p class="text-slate-500 text-sm">Carregando suas casas...</p>
        </div>
      }

      <!-- Error State -->
      @if (hasError && !isLoading) {
        <div class="max-w-md mx-auto text-center mt-16 glass-card p-8">
          <div class="text-4xl mb-4">⚠️</div>
          <h2 class="text-xl font-bold text-slate-700 mb-2">Erro ao carregar dados</h2>
          <p class="text-slate-500 mb-6">Não foi possível conectar à API. Verifique se o backend está rodando.</p>
          <button (click)="loadHouseholds()" class="btn-primary">Tentar Novamente</button>
        </div>
      }

      <!-- Main Dashboard (com casas) -->
      @if (households.length > 0 && !isLoading) {
        <main class="max-w-6xl mx-auto grid grid-cols-1 md:grid-cols-2 gap-6">
          <div (click)="router.navigate(['/shopping-lists'])" class="glass-card p-6 cursor-pointer hover:border-emerald-400 group">
            <h2 class="text-xl font-semibold text-slate-700 mb-2 group-hover:text-emerald-600 transition-colors">Listas de Compras</h2>
            <p class="text-slate-500 mb-4">Gerencie as compras da sua casa, extraia notas fiscais com IA.</p>
            <div class="text-sm text-emerald-600 font-medium">{{ listsCount }} listas &rarr;</div>
          </div>

          <div (click)="router.navigate(['/products'])" class="glass-card p-6 cursor-pointer hover:border-emerald-400 group">
            <h2 class="text-xl font-semibold text-slate-700 mb-2 group-hover:text-emerald-600 transition-colors">Catálogo de Produtos</h2>
            <p class="text-slate-500 mb-4">Veja os produtos recorrentes e histórico de preços.</p>
            <div class="text-sm text-emerald-600 font-medium">{{ productsCount }} produtos &rarr;</div>
          </div>

          <div class="glass-card p-6 cursor-pointer hover:border-emerald-400 group md:col-span-2">
            <h2 class="text-xl font-semibold text-slate-700 mb-2 group-hover:text-emerald-600 transition-colors">Gestão da Casa ({{ activeHousehold?.name }})</h2>
            <p class="text-slate-500 mb-4">Convide membros, defina permissões e gerencie esta casa.</p>
            <div class="text-sm text-emerald-600 font-medium">Ver membros &rarr;</div>
          </div>
        </main>
      }

      <!-- Empty State (sem casas) -->
      @if (households.length === 0 && !isLoading && !hasError) {
        <div class="max-w-xl mx-auto text-center mt-12 glass-card p-8">
          <h2 class="text-2xl font-bold text-slate-700 mb-2">Bem-vindo(a) ao HomeOS!</h2>
          <p class="text-slate-500 mb-6">Parece que você ainda não faz parte de nenhuma casa. Crie sua primeira casa para começar a gerenciar suas listas e produtos.</p>
          
          <div class="flex flex-col gap-3 max-w-sm mx-auto">
            <input type="text" [(ngModel)]="newHouseholdName" placeholder="Nome da sua casa (ex: Minha Casa)" class="input-glass" />
            <button (click)="createHousehold()" [disabled]="!newHouseholdName.trim() || isCreating" class="btn-primary">
              {{ isCreating ? 'Criando...' : 'Criar Casa' }}
            </button>
          </div>
          
          <div class="mt-6 pt-6 border-t border-slate-200">
            <p class="text-sm text-slate-500 mb-3">Ou recebeu um convite?</p>
            <div class="flex flex-col gap-3 max-w-sm mx-auto">
              <input type="text" [(ngModel)]="inviteCode" placeholder="Código de Convite" class="input-glass" />
              <button (click)="joinHousehold()" [disabled]="!inviteCode.trim() || isJoining" class="btn-primary bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-400 hover:to-slate-500">
                {{ isJoining ? 'Entrando...' : 'Entrar na Casa' }}
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class DashboardComponent {
  private authService = inject(AuthService);
  private householdService = inject(HouseholdService);
  private shoppingListService = inject(ShoppingListService);
  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);
  router = inject(Router);

  user = this.authService.currentUser;
  
  households: Household[] = [];
  activeHousehold: Household | null = null;
  isLoading = true;
  hasError = false;
  
  newHouseholdName = '';
  isCreating = false;
  
  inviteCode = '';
  isJoining = false;

  listsCount = 0;
  productsCount = 0;

  ngOnInit() {
    this.loadHouseholds();
    this.householdService.activeHousehold$.subscribe(h => {
      this.activeHousehold = h;
      if (h) {
        this.loadMetrics(h.id);
      }
    });
  }

  loadMetrics(householdId: string) {
    this.shoppingListService.getLists(householdId).subscribe({
      next: lists => this.listsCount = lists.length,
      error: () => {}
    });
    this.productService.getProducts(householdId).subscribe({
      next: prods => this.productsCount = prods.length,
      error: () => {}
    });
  }

  loadHouseholds() {
    this.isLoading = true;
    this.hasError = false;
    this.householdService.getMyHouseholds().subscribe({
      next: (data) => {
        this.households = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('[Dashboard] Erro ao carregar casas:', err);
        this.isLoading = false;
        this.hasError = true;
        this.cdr.markForCheck();
      }
    });
  }

  changeActiveHousehold(id: string) {
    const selected = this.households.find(h => h.id === id);
    if (selected) {
      this.householdService.setActiveHousehold(selected);
    }
  }

  createHousehold() {
    if (!this.newHouseholdName.trim()) return;
    this.isCreating = true;
    
    this.householdService.createHousehold(this.newHouseholdName).subscribe({
      next: () => {
        this.newHouseholdName = '';
        this.isCreating = false;
        this.loadHouseholds();
      },
      error: () => this.isCreating = false
    });
  }
  
  joinHousehold() {
    if (!this.inviteCode.trim()) return;
    this.isJoining = true;
    
    this.householdService.joinHousehold(this.inviteCode).subscribe({
      next: () => {
        this.inviteCode = '';
        this.isJoining = false;
        this.loadHouseholds();
      },
      error: () => this.isJoining = false
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
