import { Injectable, isDevMode } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ExamResult {
  id: number;
  studentName: string;
  score: number;
  createdAt: string;
}

export interface CreateExamResultRequest {
  studentName: string;
  score: number;
}

export interface OcrResult {
  studentName: string;
  score: number;
  rawText: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExamResultService {
  private readonly baseUrl = isDevMode() ? 'http://localhost:5197/api' : 'https://exam-reader-api.onrender.com/api';
  private readonly apiUrl = `${this.baseUrl}/exam-results`;
  private readonly ocrUrl = `${this.baseUrl}/ocr`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ExamResult[]> {
    return this.http.get<ExamResult[]>(this.apiUrl);
  }

  add(request: CreateExamResultRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }

  sendToOcr(imageFile: File): Observable<OcrResult> {
    const formData = new FormData();
    formData.append('image', imageFile);
    return this.http.post<OcrResult>(this.ocrUrl, formData);
  }
}
