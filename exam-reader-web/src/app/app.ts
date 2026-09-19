/**
 * =============================================================================
 * DOSYA: app.ts
 * AÇIKLAMA:
 * Bu dosya, uygulamamızın "beyni" olan ana bileşen (Component) sınıfını içerir.
 * 
 * Burada ne yapılır?
 * - Sayfanın mantığı (hangi ekranın gösterileceği, butonlara basılınca ne olacağı) yönetilir.
 * - Kameraya erişilip fotoğraf çekilir.
 * - Çekilen veya seçilen fotoğraflar küçültülüp (optimize edilip) sunucuya (OCR) gönderilir.
 * - Sunucudan gelen sınav notları listelenir ve yeni notlar veritabanına kaydedilir.
 * =============================================================================
 */

// 1. GEREKLİ KÜTÜPHANELERİN İÇE AKTARILMASI (IMPORTS)
import { 
  Component,          // Bu sınıfın bir Angular bileşeni olduğunu belirten ana dekoratör (özellik kazandırıcı)
  OnInit,             // Bileşen ilk açıldığında çalışacak kodları belirleyen yaşam döngüsü arayüzü (Lifecycle Hook)
  ViewChild,          // HTML şablonundaki (#videoEl, #fileInput gibi) etiketlere doğrudan erişmemizi sağlayan araç
  ElementRef,         // HTML etiketlerinin ham JavaScript karşılığına güvenli erişim sağlayan sarmalayıcı
  ChangeDetectorRef   // Sayfadaki görsel değişiklikleri Angular'ın anında ekrana yansıtmasını tetikleyen mekanizma
} from '@angular/core';

import { FormsModule } from '@angular/forms'; // [(ngModel)] iki yönlü form bağlamayı kullanabilmek için gerekli modül
import { DatePipe } from '@angular/common';   // Tarihleri kullanıcı dostu formata (dd.MM.yyyy HH:mm) çeviren boru (pipe)

// Kendi yazdığımız servis ve veri tipleri (Modeller):
import { 
  ExamResultService,         // Backend sunucusuyla iletişim kuran servisimiz
  ExamResult,                // Bir sınav sonucunun veri yapısı (id, isim, not, tarih)
  CreateExamResultRequest,   // Yeni sınav sonucu eklerken sunucuya gönderilen veri yapısı
  OcrResult                  // Yapay zekanın (OCR) resimden okuyup bize döndürdüğü veri yapısı
} from './exam-result.service';

/**
 * ADIM / SAYFA MODU TİPİ (TypeScript Union Type):
 * Uygulamamız tek bir sayfada çalışır fakat duruma göre farklı görünümlere bürünür.
 * Bu tip, 'step' değişkeninin sadece bu 6 geçerli durumdan biri olabileceğini garanti eder.
 */
type AppStep =
  | 'list'          // Kayıtlı öğrenci notlarının listelendiği ana ekran
  | 'mode-select'   // Not ekleme yöntemi seçimi (Kamera, Fotoğraf Seç veya Manuel)
  | 'camera'        // Canlı kamera önizleme ve fotoğraf çekme ekranı
  | 'ocr-loading'   // Yapay zeka fotoğrafı incelerken gösterilen yükleniyor ekranı
  | 'ocr-confirm'   // Yapay zekanın okuduğu verileri onaylama ve düzeltme formu
  | 'manual-entry'; // Tamamen klavyeyle elle not girme formu

/**
 * 2. BİLEŞEN TANIMLAYICI (DECORATOR - @Component)
 * Angular'a bu TypeScript sınıfının bir web bileşeni olduğunu ve hangi dosyalarla
 * birlikte çalıştığını tanıtır.
 */
@Component({
  selector: 'app-root',          // index.html içindeki <app-root></app-root> etiketine karşılık gelir
  standalone: true,              // Modern Angular standardı: Kendi kendine yetebilen, bağımsız bileşen
  imports: [FormsModule, DatePipe], // Bu bileşenin HTML'inde kullanılacak ek modüller
  templateUrl: './app.html',     // Bileşenin görsel arayüzünü tanımlayan HTML dosyası
  styleUrl: './app.css'          // Bileşenin süslemelerini içeren CSS dosyası
})
export class App implements OnInit {

  // ---------------------------------------------------------------------------
  // HTML ELEMANLARINA ERİŞİM (DOM REFERANSLARI - @ViewChild)
  // ---------------------------------------------------------------------------
  // app.html içindeki <video #videoEl> etiketine kod içerisinden erişmemizi sağlar:
  @ViewChild('videoEl') videoEl!: ElementRef<HTMLVideoElement>;

  // app.html içindeki <input #fileInput> gizli dosya seçiciye koddan erişmemizi sağlar:
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // ---------------------------------------------------------------------------
  // DURUM (STATE) DEĞİŞKENLERİ:
  // Bu değişkenlerin değeri değiştikçe, app.html içindeki ekran da otomatik değişir.
  // ---------------------------------------------------------------------------
  
  // Şu anda hangi ekranda olduğumuzu tutar. Varsayılan olarak liste ekranıyla başlarız.
  step: AppStep = 'list';

  // Sunucudan çekilen öğrenci sınav sonuçlarını tutan dizi (liste).
  examResults: ExamResult[] = [];

  // Veriler sunucudan beklenirken ekranda "Yükleniyor..." yazısını göstermek için bayrak.
  isLoading = true;

  // Yeni not eklerken veya OCR sonucunu onaylarken kullanılan form verisi.
  ocrForm: CreateExamResultRequest = { studentName: '', score: 0 };

  // Yapay zekanın görselde okuduğu ham, filtrelenmemiş metni tutar (kullanıcıya şeffaflık sağlamak için).
  rawText = '';

  // Canlı kamera görüntüsünün donanım akışını (stream) hafızada tutar; işimiz bitince kamerayı kapatabilmek için gereklidir.
  private stream: MediaStream | null = null;

  // ---------------------------------------------------------------------------
  // BİLDİRİM (TOAST) SİSTEMİ DEĞİŞKENLERİ:
  // ---------------------------------------------------------------------------
  notification: { message: string, type: 'success' | 'error' | null } = { message: '', type: null };
  private notificationTimeout: any; // Bildirimin birkaç saniye sonra ekrandan kaybolmasını sağlayan zamanlayıcı

  /**
   * KURUCU METOT (CONSTRUCTOR):
   * Bileşen ilk hafızaya yüklendiğinde bir kez çalışır.
   * "Dependency Injection" (Bağımlılık Enjeksiyonu) ile ihtiyacımız olan servisleri Angular'dan isteriz.
   * 
   * @param examResultService Backend API ile konuşacak servis nesnesi
   * @param cdr Değişiklikleri ekrana anında yansıtmak için Angular'ın çizim tetikleyicisi
   */
  constructor(
    private examResultService: ExamResultService,
    private cdr: ChangeDetectorRef
  ) {}

  /**
   * YAŞAM DÖNGÜSÜ METODU (ngOnInit):
   * Sayfa ilk açıldığında ve bileşen hazır olduğunda Angular bu fonksiyonu otomatik olarak çalıştırır.
   */
  ngOnInit(): void {
    // Sayfa açılır açılmaz veritabanındaki mevcut öğrenci notlarını sunucudan çek:
    this.loadResults();
  }

  // ---------------------------------------------------------------------------
  // 1. VERİLERİ SUNUCUDAN ÇEKME (READ)
  // ---------------------------------------------------------------------------
  /**
   * Backend sunucusuna HTTP GET isteği atarak tüm sınav sonuçlarını getirir.
   */
  loadResults(): void {
    this.isLoading = true; // Yükleme animasyonunu başlat

    // Servisimizin getAll metodunu çağırıyoruz. Servis bize bir "Observable" (Gözlemlenebilir akış) döner.
    // .subscribe() diyerek bu akıştan gelecek cevabı dinlemeye başlıyoruz:
    this.examResultService.getAll().subscribe({
      next: (data) => {
        // Sunucudan veriler başarıyla geldiğinde burası çalışır:
        this.examResults = data; // Gelen veriyi dizimize aktar
        this.isLoading = false;  // Yükleme animasyonunu durdur
        this.cdr.detectChanges(); // Ekranın anında yeni verilerle çizilmesini sağla
      },
      error: () => {
        // Sunucuya ulaşılamazsa veya bir hata oluşursa burası çalışır:
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // ---------------------------------------------------------------------------
  // 2. MOD VE SAYFA GEÇİŞLERİ
  // ---------------------------------------------------------------------------
  /**
   * "+ Öğrenci Notu Ekle" butonuna basıldığında mod seçme ekranını açar.
   */
  openModeSelect(): void {
    this.step = 'mode-select';
  }

  /**
   * "Manuel Ekle" seçeneğine tıklandığında formu sıfırlar ve manuel giriş ekranını açar.
   */
  openManualEntry(): void {
    this.ocrForm = { studentName: '', score: 0 }; // Form alanlarını temizle
    this.step = 'manual-entry';
  }

  // ---------------------------------------------------------------------------
  // 3. CANLI KAMERA İŞLEMLERİ
  // ---------------------------------------------------------------------------
  /**
   * Kullanıcının web kamerasına erişim izni ister ve görüntüyü ekrandaki <video> etiketine bağlar.
   */
  openCamera(): void {
    this.step = 'camera'; // Ekranı kamera moduna geçir

    // HTML'deki <video> etiketinin sayfaya tam yerleşebilmesi için 100 milisaniyelik minik bir gecikmeyle çalıştırıyoruz:
    setTimeout(() => {
      // Tarayıcının standart kamera erişim fonksiyonu:
      // 'facingMode: environment': Cep telefonlarında ön kamera yerine arka (çevre) kamerayı tercih etmesini sağlar.
      navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } })
        .then(stream => {
          // Kullanıcı izin verirse kameradan canlı video akışı (stream) gelir:
          this.stream = stream;
          // Gelen video akışını ekrandaki HTML <video> etiketinin içine aktar:
          this.videoEl.nativeElement.srcObject = stream;
        });
    }, 100);
  }

  /**
   * Canlı kamera görüntüsünden o anki kareyi yakalar (fotoğraf çeker),
   * fotoğrafı boyut olarak optimize eder (en fazla 1200px genişlik) ve OCR servisine iletir.
   */
  capturePhoto(): void {
    const video = this.videoEl.nativeElement;

    // Resim Boyutlandırma (Optimizasyon):
    // Çok yüksek çözünürlüklü fotoğraflar interneti yavaşlatır ve sunucuyu yorar.
    // Bu yüzden maksimum genişliği 1200 piksel olarak sınırlıyoruz:
    const MAX_W = 1200;
    const scale = Math.min(1, MAX_W / video.videoWidth); // Küçültme oranını hesapla

    // Hafızada sanal bir çizim tuvali (canvas) oluşturuyoruz:
    const canvas = document.createElement('canvas');
    canvas.width  = Math.round(video.videoWidth  * scale);
    canvas.height = Math.round(video.videoHeight * scale);

    // Video karesini bu sanal tuvalin üzerine çiziyoruz:
    canvas.getContext('2d')!.drawImage(video, 0, 0, canvas.width, canvas.height);

    // Fotoğraf çekildiğine göre artık canlı kamerayı kapatabiliriz:
    this.stopCamera();

    // Tuvaldeki çizimi JPEG formatında sıkıştırılmış ikili veriye (Blob) dönüştür:
    canvas.toBlob(blob => {
      if (blob) {
        // Blob verisinden sanki diskten seçilmiş gibi bir File (Dosya) nesnesi oluştur ve OCR'a gönder:
        this.sendToOcr(new File([blob], 'capture.jpg', { type: 'image/jpeg' }));
      }
    }, 'image/jpeg', 0.88); // 0.88 kalite oranı (%88 kalite hem nettir hem de dosya boyutu çok düşüktür)
  }

  /**
   * Kamerayı güvenli bir şekilde kapatır ve cihazın donanım ışığının sönmesini sağlar.
   */
  stopCamera(): void {
    if (this.stream) {
      // Kameranın tüm çalışan izlerini (tracks) tek tek durdur:
      this.stream.getTracks().forEach(t => t.stop());
      this.stream = null; // Belleği temizle
    }
  }

  // ---------------------------------------------------------------------------
  // 4. BİLGİSAYARDAN / GALERİDEN FOTOĞRAF SEÇME
  // ---------------------------------------------------------------------------
  /**
   * Gizli tuttuğumuz <input type="file"> etiketini kod ile tetikleyerek
   * kullanıcının karşısına dosya seçme penceresinin çıkmasını sağlar.
   */
  openFilePicker(): void {
    this.fileInput.nativeElement.click();
  }

  /**
   * Kullanıcı bilgisayarından veya telefon galerisinden bir resim seçtiğinde çalışır.
   * Seçilen resmi okur, 1200px sınırına göre optimize eder ve OCR'a iletir.
   * 
   * @param event Dosya seçme olayının detayları
   */
  onFileSelected(event: Event): void {
    // Seçilen ilk dosyayı yakala:
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return; // Kullanıcı dosya seçmeden pencereyi kapattıysa hiçbir şey yapma

    // Resmi tarayıcı hafızasında açmak için geçici bir görsel nesnesi oluştur:
    const img = new Image();
    img.onload = () => {
      // Resim hafızaya tam yüklendiğinde boyutlarını ölç ve gerekirse küçült:
      const MAX_W = 1200;
      const scale = Math.min(1, MAX_W / img.width);

      const canvas = document.createElement('canvas');
      canvas.width = Math.round(img.width * scale);
      canvas.height = Math.round(img.height * scale);

      const ctx = canvas.getContext('2d');
      ctx?.drawImage(img, 0, 0, canvas.width, canvas.height);

      // Optimize edilen resmi JPEG dosyası olarak paketle ve OCR'a gönder:
      canvas.toBlob(blob => {
        if (blob) {
          this.sendToOcr(new File([blob], file.name, { type: file.type || 'image/jpeg' }));
        }
      }, file.type || 'image/jpeg', 0.88);
    };

    // Dosyayı tarayıcının okuyabileceği geçici bir web adresine çevir:
    img.src = URL.createObjectURL(file);
  }

  // ---------------------------------------------------------------------------
  // 5. BİLDİRİM (TOAST) GÖSTERME FONKSİYONU
  // ---------------------------------------------------------------------------
  /**
   * Kullanıcıya ekranda 4 saniye boyunca kalan renkli bir bilgilendirme kutusu gösterir.
   * 
   * @param message Gösterilecek metin mesajı
   * @param type 'success' (Yeşil / Başarılı) veya 'error' (Kırmızı / Hata)
   */
  showNotification(message: string, type: 'success' | 'error'): void {
    this.notification = { message, type };

    // Eğer daha önceden ayarlanmış bir zamanlayıcı varsa sıfırla:
    if (this.notificationTimeout) {
      clearTimeout(this.notificationTimeout);
    }

    // 4000 milisaniye (4 saniye) sonra bildirimi otomatik olarak kapat:
    this.notificationTimeout = setTimeout(() => {
      this.notification.type = null; // Bildirimi gizle
      this.cdr.detectChanges();      // Ekranı güncelle
    }, 4000);

    this.cdr.detectChanges();
  }

  // ---------------------------------------------------------------------------
  // 6. YAPAY ZEKA (OCR) İŞLEMİ
  // ---------------------------------------------------------------------------
  /**
   * Hazırlanan fotoğraf dosyasını sunucudaki OCR servisine gönderir.
   * Gelen cevaba göre formu otomatik doldurur ve kullanıcı onay ekranına geçer.
   * 
   * @param file Gönderilecek resim dosyası
   */
  sendToOcr(file: File): void {
    this.step = 'ocr-loading'; // Ekranda yükleniyor animasyonunu göster

    // Servis üzerinden sunucuya HTTP POST isteği gönderiyoruz:
    this.examResultService.sendToOcr(file).subscribe({
      next: (result: OcrResult) => {
        // Yapay zeka fotoğrafı okumayı başardı:
        // Fotoğraftan okunan öğrenci adı ve notu formumuza otomatik yazıyoruz:
        this.ocrForm = { studentName: result.studentName, score: result.score };
        this.rawText = result.rawText; // Yapay zekanın algıladığı ham metin

        // Eğer hiçbir şey okunamadıysa kullanıcıya uyarı göster:
        if (result.score === 0 && result.studentName.trim() === '') {
          this.showNotification('Yazı tam anlaşılamadı. Lütfen kontrol edin.', 'error');
        }

        // Kullanıcının sonuçları görmesi ve düzeltebilmesi için onay ekranını aç:
        this.step = 'ocr-confirm';
        this.cdr.detectChanges();
      },
      error: () => {
        // İnternet koparsa veya sunucuda bir arıza olursa:
        this.showNotification('OCR işlemi başarısız oldu. Lütfen tekrar deneyin.', 'error');
        this.step = 'mode-select'; // Mod seçim ekranına geri dön
        this.cdr.detectChanges();
      }
    });
  }

  // ---------------------------------------------------------------------------
  // 7. KAYDETME (CREATE) VE İPTAL İŞLEMLERİ
  // ---------------------------------------------------------------------------
  /**
   * Formdaki verileri (Öğrenci Adı ve Not) backend sunucusuna kalıcı olarak kaydeder.
   */
  save(): void {
    // Servisimizin add metodunu çağırarak veritabanına kayıt isteği atıyoruz:
    this.examResultService.add(this.ocrForm).subscribe({
      next: () => {
        // Başarılı olduğunda kullanıcıya yeşil kutlama mesajı göster:
        this.showNotification('✅ Öğrenci notu başarıyla eklendi.', 'success');
        this.step = 'list'; // Ana liste ekranına geri dön
        this.loadResults(); // Eklenen yeni veriyi de görmek için listeyi baştan çek
      },
      error: () => {
        // Kaydedilirken bir problem olursa kullanıcıya bildir:
        this.showNotification('Kaydedilirken bir hata oluştu.', 'error');
      }
    });
  }

  /**
   * Kullanıcı herhangi bir aşamada "İptal" butonuna basarsa çalışır.
   * Açık kamerayı kapatır ve ana liste ekranına güvenle geri döner.
   */
  cancel(): void {
    this.stopCamera(); // Kamera açıksa kapat
    this.step = 'list';  // Ana ekrana dön
  }
}
