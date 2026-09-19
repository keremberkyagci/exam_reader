// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// "namespace" (ad alanı), projedeki kodları düzenli tutmak için kullanılan sanal bir klasör gibidir.
// Tıpkı bilgisayarınızdaki klasörler gibi, farklı sınıfların (class) karışmasını önler ve onları gruplar.
// Burada "ExamReader.Api.Models" diyerek, bu dosyanın "ExamReader" projesinin "Api" katmanındaki
// "Models" (Veri Modelleri) klasörüne ait olduğunu belirtiyoruz.
namespace ExamReader.Api.Models;

// =============================================================================
// SINIF (CLASS) TANIMI - DTO (Data Transfer Object / Veri Transfer Nesnesi)
// =============================================================================
// "public": Bu sınıfa projenin her yerinden (diğer dosyalardan) erişilebileceğini belirtir ("herkese açık").
// "class": C# dilinde bir nesnenin şablonunu/kalıbını oluşturmak için kullanılan anahtar kelimedir.
//
// Bu sınıf bir "DTO" (Veri Taşıma Nesnesi) görevi görür.
// Kullanıcı (veya arayüz) sisteme yeni bir sınav sonucu eklemek istediğinde,
// sadece gerekli olan bilgileri (öğrencinin adı ve puanı) paketleyip sunucuya göndermek için bu kalıbı kullanır.
public class CreateExamResultRequest
{
    // =========================================================================
    // ÖĞRENCİ ADI ÖZELLİĞİ (PROPERTY)
    // =========================================================================
    // "string": Metin, yazı, harf dizilerini saklamak için kullanılan veri tipidir (Örn: "Ali Yılmaz").
    // "StudentName": Özelliğin adı; öğrencinin adını ve soyadını temsil eder.
    // "{ get; set; }": C#'a özgü bir yapıdır.
    //   - "get" (al/oku): Bu bilginin okunabilmesini sağlar.
    //   - "set" (ayarla/yaz): Bu bilgiye yeni bir değer atanabilmesini sağlar.
    // "= string.Empty;": Başlangıç değeri atamasıdır.
    //   Eğer kullanıcı bir isim girmezse değer "null" (tanımsız/boşluk) kalıp programda hataya yol açmasın diye,
    //   içerisine en baştan boş bir metin ("") koyuyoruz.
    public string StudentName { get; set; } = string.Empty;

    // =========================================================================
    // SINAV PUANI ÖZELLİĞİ (PROPERTY)
    // =========================================================================
    // "int": "Integer" kelimesinin kısaltmasıdır; tam sayıları saklamak için kullanılır (Örn: 0, 50, 100).
    // "Score": Öğrencinin sınavdan aldığı notu / puanı temsil eden değişken/özellik adıdır.
    // "{ get; set; }": Puan değerinin hem okunabilmesini (get) hem de güncellenebilmesini (set) sağlar.
    public int Score { get; set; }
}
