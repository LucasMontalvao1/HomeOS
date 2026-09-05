import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductService, Product } from '../../core/product.service';
import { HouseholdService } from '../../core/household.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-mesh min-h-screen p-6">
      <nav class="glass-card mb-8 p-4 flex justify-between items-center">
        <div class="flex items-center gap-4">
          <button (click)="router.navigate(['/dashboard'])" class="text-slate-500 hover:text-emerald-600 font-medium text-sm flex items-center gap-1">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/></svg>
            Voltar
          </button>
          <h1 class="text-xl font-bold text-emerald-600 ml-4 border-l border-slate-200 pl-4">Catálogo de Produtos</h1>
        </div>
        <button (click)="openModal()" class="btn-primary text-sm px-4 flex items-center gap-2">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
          Novo Produto
        </button>
      </nav>

      <main class="max-w-6xl mx-auto">
        @if (isLoading) {
          <div class="flex flex-col items-center justify-center mt-24 gap-4">
            <div class="w-8 h-8 border-4 border-emerald-400 border-t-transparent rounded-full animate-spin"></div>
            <p class="text-slate-500 text-sm">Carregando produtos...</p>
          </div>
        }

        @if (!isLoading && products.length === 0) {
          <div class="text-center py-16 glass-card">
            <div class="text-5xl mb-4">📦</div>
            <p class="text-slate-600 font-semibold mb-1">Nenhum produto cadastrado</p>
            <p class="text-slate-400 text-sm mb-6">Cadastre os produtos que você costuma comprar para usá-los nas listas.</p>
            <button (click)="openModal()" class="btn-primary text-sm px-6">Cadastrar meu primeiro produto</button>
          </div>
        }

        @if (!isLoading && products.length > 0) {
          <!-- Search bar -->
          <div class="mb-6">
            <input type="text" [(ngModel)]="searchQuery" (ngModelChange)="filterProducts()"
              placeholder="Buscar produto por nome, marca ou categoria..."
              class="input-glass w-full max-w-lg" />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            @for (p of filtered; track p.id) {
              <div class="glass-card p-5 group relative">
                <div class="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity flex gap-2">
                  <button (click)="openModal(p)" class="text-slate-400 hover:text-emerald-500 p-1 bg-white/80 rounded shadow-sm">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/></svg>
                  </button>
                  <button (click)="deleteProduct(p.id)" class="text-slate-400 hover:text-red-500 p-1 bg-white/80 rounded shadow-sm">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                  </button>
                </div>

                <!-- Icon or Image placeholder -->
                @if (p.imageUrl) {
                  <div class="w-full h-32 bg-slate-100 rounded-xl mb-4 overflow-hidden">
                    <img [src]="p.imageUrl" [alt]="p.name" class="w-full h-full object-cover" (error)="$event.target.src = 'https://placehold.co/400x300/e2e8f0/64748b?text=Sem+Foto'" />
                  </div>
                } @else {
                  <div class="w-full h-24 bg-gradient-to-br from-emerald-50 to-teal-100 rounded-xl mb-4 flex items-center justify-center text-emerald-300">
                    <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"/></svg>
                  </div>
                }

                @if (p.category) {
                  <span class="inline-block px-2 py-0.5 bg-emerald-50 text-emerald-700 text-xs rounded-full font-medium mb-2">{{ p.category }}</span>
                }
                <h3 class="font-bold text-slate-700 text-sm leading-tight">{{ p.name }}</h3>
                @if (p.brand) {
                  <p class="text-xs text-slate-400 mt-1">{{ p.brand }}</p>
                }
                @if (p.unit) {
                  <p class="text-xs text-slate-400">Unidade: {{ p.unit }}</p>
                }
              </div>
            }
          </div>
        }
      </main>

      <!-- Modal Add/Edit -->
      @if (isModalOpen) {
        <div class="fixed inset-0 bg-slate-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div class="glass-card w-full max-w-md p-6 bg-white/95">
            <h2 class="text-xl font-bold text-slate-700 mb-6">{{ editingId ? 'Editar Produto' : 'Novo Produto' }}</h2>

            <div class="flex flex-col gap-4">
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Nome do Produto <span class="text-red-400">*</span></label>
                <input type="text" [(ngModel)]="formData.name" class="input-glass" placeholder="Ex: Arroz Tio João 5kg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Marca (opcional)</label>
                <input type="text" [(ngModel)]="formData.brand" class="input-glass" placeholder="Ex: Tio João" />
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-sm font-medium text-slate-700 mb-1">Categoria (opcional)</label>
                  <input type="text" [(ngModel)]="formData.category" class="input-glass" placeholder="Ex: Mercearia" />
                </div>
                <div>
                  <label class="block text-sm font-medium text-slate-700 mb-1">Unidade (opcional)</label>
                  <input type="text" [(ngModel)]="formData.unit" class="input-glass" placeholder="Ex: kg, L, un" />
                </div>
              </div>
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">Código de Barras (opcional)</label>
                <input type="text" [(ngModel)]="formData.barcode" class="input-glass" placeholder="Ex: 7891234567890" />
              </div>
              <div>
                <label class="block text-sm font-medium text-slate-700 mb-1">URL da Foto (opcional)</label>
                <input type="text" [(ngModel)]="formData.imageUrl" class="input-glass" placeholder="https://exemplo.com/foto.jpg" />
                @if (formData.imageUrl) {
                  <div class="mt-2 rounded-lg overflow-hidden h-24 w-24 bg-slate-100 border border-slate-200">
                    <img [src]="formData.imageUrl" class="w-full h-full object-cover" (error)="$event.target.src = 'https://placehold.co/400x300/e2e8f0/64748b?text=Erro'" />
                  </div>
                }
              </div>

              @if (modalError) {
                <div class="text-sm text-red-500 bg-red-50 px-3 py-2 rounded-lg">{{ modalError }}</div>
              }
            </div>

            <div class="flex justify-end gap-3 mt-8">
              <button (click)="closeModal()" class="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800">Cancelar</button>
              <button (click)="saveProduct()" [disabled]="isSaving" class="btn-primary py-2 text-sm">
                {{ isSaving ? 'Salvando...' : 'Salvar Produto' }}
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class ProductsComponent {
  router = inject(Router);
  private productService = inject(ProductService);
  private householdService = inject(HouseholdService);
  private cdr = inject(ChangeDetectorRef);

  products: Product[] = [];
  filtered: Product[] = [];
  isLoading = true;
  searchQuery = '';

  isModalOpen = false;
  isSaving = false;
  modalError = '';

  editingId: string | null = null;
  formData = { name: '', brand: '', category: '', unit: '', barcode: '', imageUrl: '' };

  ngOnInit() {
    this.loadProducts();
  }

  get activeHouseholdId() {
    return this.householdService.activeHouseholdId;
  }

  loadProducts() {
    if (!this.activeHouseholdId) return;
    this.isLoading = true;
    this.productService.getProducts(this.activeHouseholdId).subscribe({
      next: (data) => {
        this.products = data;
        this.filtered = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  filterProducts() {
    const q = this.searchQuery.toLowerCase().trim();
    if (!q) {
      this.filtered = this.products;
      return;
    }
    this.filtered = this.products.filter(p =>
      p.name.toLowerCase().includes(q) ||
      (p.brand || '').toLowerCase().includes(q) ||
      (p.category || '').toLowerCase().includes(q)
    );
  }

  openModal(product?: Product) {
    if (product) {
      this.editingId = product.id;
      this.formData = {
        name: product.name,
        brand: product.brand || '',
        category: product.category || '',
        unit: product.unit || '',
        barcode: product.barcode || '',
        imageUrl: product.imageUrl || ''
      };
    } else {
      this.editingId = null;
      this.formData = { name: '', brand: '', category: '', unit: '', barcode: '', imageUrl: '' };
    }
    this.modalError = '';
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  saveProduct() {
    if (!this.formData.name.trim()) {
      this.modalError = 'Nome do produto é obrigatório.';
      return;
    }
    if (!this.activeHouseholdId) return;

    this.isSaving = true;
    const payload = {
      name: this.formData.name.trim(),
      brand: this.formData.brand.trim() || undefined,
      category: this.formData.category.trim() || undefined,
      unit: this.formData.unit.trim() || undefined,
      barcode: this.formData.barcode.trim() || undefined,
      imageUrl: this.formData.imageUrl.trim() || undefined
    };

    const req = this.editingId
      ? this.productService.updateProduct(this.editingId, payload, this.activeHouseholdId)
      : this.productService.createProduct(payload, this.activeHouseholdId);

    req.subscribe({
      next: () => {
        this.isSaving = false;
        this.closeModal();
        this.loadProducts();
      },
      error: () => {
        this.isSaving = false;
        this.modalError = 'Erro ao salvar produto.';
      }
    });
  }

  deleteProduct(id: string) {
    if (!confirm('Tem certeza que deseja excluir este produto?') || !this.activeHouseholdId) return;
    this.productService.deleteProduct(id, this.activeHouseholdId).subscribe({
      next: () => this.loadProducts()
    });
  }
}
