# Implementasi Algoritma Greedy pada Pembuatan Bot Robocode Tank Royale

![C#](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp&logoColor=white&labelColor=555555)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet&logoColor=white&labelColor=555555)

## 📌 Deskripsi Singkat

Proyek ini berisi empat bot AI yang dikembangkan menggunakan bahasa C# dan framework **Robocode Tank Royale**. Masing-masing bot mengimplementasikan varian strategi **Greedy Algorithm** untuk memaksimalkan skor melalui komponen seperti **bullet damage**, **ram damage**, dan **survival score**. Strategi utama yang digunakan dalam implementasi akhir adalah **Greedy Strategy 1** (Made in China), yang terbukti unggul dalam pengujian.

## 🤖 Strategi Greedy Tiap Bot

### 1. **Made In China** (Strategi 1 - First Scanned)
Bot melakukan scanning secara terus-menerus untuk mendeteksi musuh. Jika menemukan bot musuh, bot akan:
- Mendekati musuh hingga mencapai jarak aman.
- Melakukan **locking** dan menembak dengan kekuatan optimal (power tergantung jarak).
- Menjaga jarak dari musuh jika terlalu dekat.
- Bot terus menargetkan musuh pertama yang terdeteksi dan tidak berhenti menembak sampai musuh hilang dari radar.

### 2. **EDI** (Strategi 2 - Weakest Target)
Bot akan memilih **musuh dengan energi terendah** menggunakan 360° scan. Setelah itu, bot:
- Melakukan **predictive locking** dan firing berdasarkan estimasi posisi musuh.
- Mengincar bonus poin dari **bullet/ram damage bonus**.
- Menyerang secara agresif namun rentan bila banyak musuh mendekat.

### 3. **Hari Styles** (Strategi 3 - Concentrated Sector)
Bot melakukan 360° radar scan dan membagi medan perang menjadi **12 sektor (30°)**.
- Menentukan sektor dengan musuh terbanyak (cluster).
- Melakukan spray dengan bullet power maksimal ke sektor tersebut.
- Jika terkena tembakan, bot bergerak ke sektor dengan jumlah musuh paling sedikit (survival mode).

### 4. **Steve** (Strategi 4 - Survivorship)
Bot difokuskan untuk bertahan hidup:
- Menghindari tembok dan tembakan musuh.
- Menyerang hanya jika kondisi memungkinkan (musuh lemah atau lambat).
- Memiliki pergerakan random dan tidak mudah diprediksi oleh lawan.

## ✅ Requirement & Instalasi

- **.NET SDK**: Minimal versi `6.0`
- **Robocode Tank Royale GUI**: Unduh dari [Starter Pack Resmi](https://github.com/Ariel-HS/tubes1-if2211-starter-pack/releases)
- **BotAPI Version**: `0.30.0`
- **OS**: Windows / Linux / Mac (sesuai dengan GUI dan .NET kompatibilitas)

## ⚙️ Cara Build & Compile

1. Pastikan semua file berikut ada:
   - `NamaBot.cs`
   - `NamaBot.json`
   - `NamaBot.csproj`
   - `NamaBot.cmd` / `NamaBot.sh`

2. Jalankan salah satu dari perintah berikut:
   - **Windows**:
     ```cmd
     ./NamaBot.cmd
     ```
   - **Linux / Mac**:
     ```bash
     ./NamaBot.sh
     ```

3. Jalankan GUI:
   ```bash
   java -jar robocode-tankroyale-gui-0.30.0.jar
   ```

4. Boot dan tambahkan bot ke pertandingan melalui GUI.

📌 Jika bot tidak muncul di GUI, pastikan versi .NET sesuai dan path tidak memiliki spasi.

## 👨‍💻 Author
### Tim Strategi Algoritma LASTWORGHXHXX 2025 - Kelas IF-2211

Nathaniel Jonathan Rusli - 13523013

Maheswara Bayu Kaindra - 13523015

Peter Wongsoredjo - 13523039

