import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PaginationResponse } from '../../core/models/pagination-response.model';
import { WordModel } from '../../core/models/word.model';
import { WordsApiService } from '../../core/services/words-api.service';
import { mapApiError } from '../../core/utils/map-api-error';

@Component({
  selector: 'app-dictionary-page',
  imports: [CommonModule],
  templateUrl: './dictionary-page.component.html',
  styleUrl: './dictionary-page.component.css'
})
export class DictionaryPageComponent implements OnInit {
  private readonly wordsApi = inject(WordsApiService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly pageSize = environment.defaultPageSize;

  isLoading = false;
  errorMessage: string | null = null;

  items: WordModel[] = [];
  pagination: PaginationResponse | null = null;

  isDownloading = false;

  ngOnInit(): void {
    void this.loadPage(1);
  }

  async loadPage(page: number): Promise<void> {
    const safePage = Math.max(1, page);

    this.isLoading = true;
    this.errorMessage = null;

    try {
      const response = await firstValueFrom(this.wordsApi.getPagedWords(safePage, this.pageSize));

      this.pagination = response;
      this.items = response.items;

      if (response.totalPages > 0 && response.currentPage > response.totalPages) {
        await this.loadPage(response.totalPages);
      }
    } catch (error) {
      this.errorMessage = mapApiError(error, 'Failed to load dictionary page.');
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  async deleteWord(id: number): Promise<void> {
    const shouldDelete = window.confirm(`Delete word with id ${id}?`);

    if (!shouldDelete) {
      return;
    }

    this.errorMessage = null;

    try {
      await firstValueFrom(this.wordsApi.deleteWord(id));

      const currentPage = this.pagination?.currentPage ?? 1;
      await this.loadPage(currentPage);

      if (this.items.length === 0 && currentPage > 1) {
        await this.loadPage(currentPage - 1);
      }
    } catch (error) {
      this.errorMessage = mapApiError(error, 'Failed to delete word.');
      this.cdr.detectChanges();
    }
  }

  goToAnagrams(word: string): void {
    void this.router.navigate(['/anagrams'], { queryParams: { word } });
  }

  async download(): Promise<void> {
    const fileName = 'zodynas.txt';

    this.isDownloading = true;
    this.errorMessage = null;

    try {
      const file = await firstValueFrom(this.wordsApi.downloadDictionaryFile(fileName));
      const objectUrl = URL.createObjectURL(file);
      const anchor = document.createElement('a');

      anchor.href = objectUrl;
      anchor.download = fileName;
      anchor.click();

      URL.revokeObjectURL(objectUrl);
    } catch (error) {
      this.errorMessage = mapApiError(error, 'Failed to download dictionary file.');
    } finally {
      this.isDownloading = false;
      this.cdr.detectChanges();
    }
  }

  get currentPage(): number {
    return this.pagination?.currentPage ?? 1;
  }

  get totalPages(): number {
    return this.pagination?.totalPages ?? 1;
  }

  get hasPreviousPage(): boolean {
    return this.pagination?.hasPreviousPage ?? false;
  }

  get hasNextPage(): boolean {
    return this.pagination?.hasNextPage ?? false;
  }
}
