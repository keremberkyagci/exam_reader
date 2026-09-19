/**
 * =============================================================================
 * DOSYA: exam-result.service.ts
 * AÇIKLAMA:
 * Bu dosya, Angular uygulamamız ile uzaktaki sunucu (Backend / Web API) arasında
 * köprü görevi gören bir "Servis" (Service) dosyasıdır.
 * 
 * Servis Nedir?
 * Angular'da bileşenler (Components) sadece ekranı yönetmekle ilgilenmelidir.
 * İnternetten veri çekmek, sunucuya veri göndermek gibi "arka plan işleri" servislere
 * devredilir. Böylece kodumuz düzenli, temiz ve tekrar kullanılabilir olur.
 * =============================================================================
 */

// 1. İÇE AKTARMALAR (IMPORTS)
import { Injectable } from '@angular/core';  // Bu sınıfın bir "servis" olarak enjekte edilebilmesini sağlayan dekoratör
import { HttpClient } from '@angular/common/http'; // Tarayıcı üzerinden internete HTTP (GET, POST vb.) istekleri atan araç
import { Observable } from 'rxjs';          // Asenkron (zaman alan) veri akışlarını temsil eden RxJS yapısı

/**
 * ---------------------------------------------------------------------------
 * ARAYÜZLER (INTERFACES - VERİ MODELLERİ)
 * TypeScript'te interface'ler, verilerin "şablonunu" veya "kalıbını" belirler.
 * Hangi özelliğin hangi tipte (metin, sayı vb.) olması gerektiğini kurallara bağlar.
 * ---------------------------------------------------------------------------
 */

/**
 * Sunucudan gelen kayıtlı bir sınav sonucunun yapısı:
 */
export interface ExamResult {
  id: number;          // Kaydın veritabanındaki benzersiz sıra numarası (örn: 1, 2, 45)
  studentName: string; // Öğrencinin adı ve soyadı (metin)
  score: number;       // Sınavdan aldığı not (sayı, örn: 85)
  createdAt: string;   // Kaydın oluşturulma tarihi ve saati (metin/ISO formatı)
}

/**
 * Yeni bir sınav sonucu eklerken sunucuya göndereceğimiz istek paketi:
 * (ID ve Tarih sunucuda otomatik oluşturulduğu için burada sadece ad ve not gereklidir)
 */
export interface CreateExamResultRequest {
  studentName: string; // Eklenecek öğrencinin adı
  score: number;       // Eklenecek öğrencinin notu
}

/**
 * Fotoğrafı gönderdikten sonra yapay zekanın (OCR) bize döndürdüğü yanıt paketi:
 */
export interface OcrResult {
  studentName: string; // Yapay zekanın görselden okuduğu öğrenci adı
  score: number;       // Yapay zekanın görselden okuduğu sınav notu
  rawText: string;     // Yapay zekanın resim üzerinde gördüğü tüm ham metin satırları
}

/**
 * ---------------------------------------------------------------------------
 * SERVİS SINIFI (@Injectable)
 * 
 * 'providedIn: root': Bu servisin "Tekil" (Singleton) olarak tüm uygulama genelinde
 * tek bir kopyasının üretileceğini ve her bileşenin aynı kopyayı paylaşacağını belirtir.
 * ---------------------------------------------------------------------------
 */
@Injectable({
  providedIn: 'root'
})
export class ExamResultService {

  // Sunucumuzun (Render üzerinde barındırılan C# ASP.NET Core API'mizin) temel web adresi:
  private readonly baseUrl = 'https://exam-reader-g3xb.onrender.com/api';

  // Sınav sonuçlarını listelemek ve yeni eklemek için kullanılan API uç noktası:
  private readonly apiUrl = `${this.baseUrl}/exam-results`;

  // Fotoğrafları gönderip yapay zekaya okutacağımız OCR uç noktası:
  private readonly ocrUrl = `${this.baseUrl}/ocr`;

  /**
   * KURUCU METOT (CONSTRUCTOR):
   * Angular, 'HttpClient' aracını otomatik olarak buraya enjekte eder (Dependency Injection).
   * 'private http: HttpClient' ifadesiyle bu sınıf içinde 'this.http' diyerek istek atabiliriz.
   */
  constructor(private http: HttpClient) {}

  /**
   * 1. TÜM SINAV SONUÇLARINI GETİR (GET Request)
   * 
   * Sunucuya bir HTTP GET isteği gönderir.
   * Sunucu bize 'ExamResult' kalıbına uyan bir liste (dizi: ExamResult[]) döner.
   * 
   * Observable Nedir?
   * İnternet istekleri anında bitmez; birkaç saniye sürebilir.
   * "Observable" (Gözlemlenebilir akış), tıpkı bir gazete aboneliği gibidir.
   * Sunucudan veri geldiği anda bizi haberdar eder (haber verir).
   * 
   * @returns Observable<ExamResult[]> Sınav sonuçları listesi akışı
   */
  getAll(): Observable<ExamResult[]> {
    return this.http.get<ExamResult[]>(this.apiUrl);
  }

  /**
   * 2. YENİ BİR SINAV SONUCU EKLE (POST Request)
   * 
   * Sunucuya JSON formatında bir öğrenci notu gönderir ve veritabanına kaydettirir.
   * 
   * @param request Eklenecek öğrencinin adı ve notunu içeren nesne
   * @returns Sunucunun işlem yanıtı
   */
  add(request: CreateExamResultRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }

  /**
   * 3. FOTOĞRAFI YAPAY ZEKA (OCR) SERVİSİNE GÖNDER (POST with Multipart Form-Data)
   * 
   * Bir resim dosyasını sunucuya göndermek için standart JSON formatı yetersizdir.
   * Bunun yerine HTML formlarının dosya yükleme biçimi olan 'FormData' (Çok parçalı form) kullanılır.
   * 
   * @param imageFile Kullanıcının çektiği veya seçtiği resim dosyası (File)
   * @returns Observable<OcrResult> Yapay zekanın okuduğu ad, not ve ham metin
   */
  sendToOcr(imageFile: File): Observable<OcrResult> {
    // Sanal bir form oluşturuyoruz:
    const formData = new FormData();

    // Bu sanal formun içine 'image' anahtarıyla resim dosyasını yerleştiriyoruz:
    // (Sunucudaki C# API bu dosyayı "image" adıyla beklemektedir)
    formData.append('image', imageFile);

    // Formu HTTP POST isteğiyle OCR uç noktasına yolluyoruz:
    return this.http.post<OcrResult>(this.ocrUrl, formData);
  }
}
