import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ExamResultService, ExamResult, CreateExamResultRequest, OcrResult } from './exam-result.service';

type AppStep =
  | 'list'          // normal liste görünümü
  | 'mode-select'   // Kamerayı Aç / Fotoğraf Seç
  | 'camera'        // kamera canlı görüntü
  | 'ocr-loading'   // OCR bekleniyor
  | 'ocr-confirm'   // OCR sonucu onaylama formu
  | 'manual-entry'; // Manuel giriş formu

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule, DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  @ViewChild('videoEl') videoEl!: ElementRef<HTMLVideoElement>;
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  step: AppStep = 'list';
  examResults: ExamResult[] = [];
  isLoading = true;

  // OCR/Manuel onay formu
  ocrForm: CreateExamResultRequest = { studentName: '', score: 0 };
  rawText = '';

  private stream: MediaStream | null = null;

  constructor(
    private examResultService: ExamResultService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadResults();
  }

  loadResults(): void {
    this.isLoading = true;
    this.examResultService.getAll().subscribe({
      next: (data) => {
        this.examResults = data;
        this.isLoading = false;
        this.cdr.detectChanges(); // Ekranı anında güncelle
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Butona basıldı → mod seçimi
  openModeSelect(): void {
    this.step = 'mode-select';
  }

  // Manuel Ekle
  openManualEntry(): void {
    this.ocrForm = { studentName: '', score: 0 };
    this.step = 'manual-entry';
  }

  // --- Kamera ---
  openCamera(): void {
    this.step = 'camera';
    setTimeout(() => {
      navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } })
        .then(stream => {
          this.stream = stream;
          this.videoEl.nativeElement.srcObject = stream;
        });
    }, 100);
  }

  capturePhoto(): void {
    const video = this.videoEl.nativeElement;
    const MAX_W = 1200;
    const scale = Math.min(1, MAX_W / video.videoWidth);
    const canvas = document.createElement('canvas');
    canvas.width  = Math.round(video.videoWidth  * scale);
    canvas.height = Math.round(video.videoHeight * scale);
    canvas.getContext('2d')!.drawImage(video, 0, 0, canvas.width, canvas.height);
    this.stopCamera();

    canvas.toBlob(blob => {
      if (blob) this.sendToOcr(new File([blob], 'capture.jpg', { type: 'image/jpeg' }));
    }, 'image/jpeg', 0.88);
  }

  stopCamera(): void {
    this.stream?.getTracks().forEach(t => t.stop());
    this.stream = null;
  }

  // --- Dosya Seç ---
  openFilePicker(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    // Fotoğrafı küçült (max 1200px)
    const img = new Image();
    img.onload = () => {
      const MAX_W = 1200;
      const scale = Math.min(1, MAX_W / img.width);
      const canvas = document.createElement('canvas');
      canvas.width = Math.round(img.width * scale);
      canvas.height = Math.round(img.height * scale);
      const ctx = canvas.getContext('2d');
      ctx?.drawImage(img, 0, 0, canvas.width, canvas.height);

      canvas.toBlob(blob => {
        if (blob) this.sendToOcr(new File([blob], file.name, { type: file.type || 'image/jpeg' }));
      }, file.type || 'image/jpeg', 0.88);
    };
    img.src = URL.createObjectURL(file);
  }

  // Bildirim (Toast) Sistemi
  notification: { message: string, type: 'success' | 'error' | null } = { message: '', type: null };
  private notificationTimeout: any;

  showNotification(message: string, type: 'success' | 'error'): void {
    this.notification = { message, type };
    if (this.notificationTimeout) clearTimeout(this.notificationTimeout);
    this.notificationTimeout = setTimeout(() => {
      this.notification.type = null;
      this.cdr.detectChanges();
    }, 4000);
    this.cdr.detectChanges();
  }

  // --- OCR ---
  sendToOcr(file: File): void {
    this.step = 'ocr-loading';
    this.examResultService.sendToOcr(file).subscribe({
      next: (result: OcrResult) => {
        this.ocrForm = { studentName: result.studentName, score: result.score };
        this.rawText = result.rawText;
        if (result.score === 0 && result.studentName.trim() === '') {
          this.showNotification('Yazı tam anlaşılamadı. Lütfen kontrol edin.', 'error');
        }
        this.step = 'ocr-confirm';
        this.cdr.detectChanges();
      },
      error: () => {
        this.showNotification('OCR işlemi başarısız oldu. Lütfen tekrar deneyin.', 'error');
        this.step = 'mode-select';
        this.cdr.detectChanges();
      }
    });
  }

  // --- Kaydet ---
  save(): void {
    this.examResultService.add(this.ocrForm).subscribe({
      next: () => {
        this.showNotification('✅ Öğrenci notu başarıyla eklendi.', 'success');
        this.step = 'list';
        this.loadResults();
      },
      error: () => {
        this.showNotification('Kaydedilirken bir hata oluştu.', 'error');
      }
    });
  }

  cancel(): void {
    this.stopCamera();
    this.step = 'list';
  }
}
