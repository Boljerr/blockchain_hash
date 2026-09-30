# Blockchain maišos funkcijos: Igno ir HashaMasha

Dvi atskiros savos 256 bitų maišos funkcijų realizacijos: **Igno** (Ignas Šimaitis) ir **HashaMasha**, rezultatuose – **Aivaro** (Aivaras Jomantas). Algoritmai nekviečia jokių standartinių maišos funkcijų; MD5, SHA-1 ir SHA-256 naudojami tik palyginimui.

## 1. Paleidimas

Reikia [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build blockchain_hash.sln                  # sukompiliuoti
dotnet run --project Hash                         # interaktyvi programa
cd Hash.Experiments && dotnet run -c Release      # visi eksperimentai
```

- **Įvesties režimai:**
  - `1` – ranka įvestas tekstas;
  - `2` – failas pagal nurodytą kelią;
  - `3` – pavyzdinis failas `data/test.txt`.

  Tada pasirenkama realizacija: `1` – Igno, `2` – Aivaro.
- **Klaidos pranešamos:**
  - neteisingas pasirinkimas – „Invalid choice. Try again.“;
  - neegzistuojantis failas ar skaitymo klaida – išspausdinamas pranešimas ir kelio prašoma iš naujo;
  - uždaryta įvestis – „Console input was closed.“
- **Eksperimentai:**
  - kurie paleidžiami, nustatoma [`Program.cs`](Hash.Experiments/Program.cs);
  - 2 eksperimentas paprašo įvesti `ABC`;
  - rezultatai įrašomi į `Hash.Experiments/Results/*.csv`.

## 2. Įvestis ir išvestis

- **Baitai:** ranka įvestas tekstas koduojamas UTF-8 be BOM, o failas maišomas tiksliai toks, koks išsaugotas (įskaitant BOM, jei jis yra).
- **Tarpai ir eilučių pabaigos:**
  - tarpai išlaikomi;
  - ranka įvestos eilutės pabaigos simbolis neįtraukiamas;
  - faile `\n` ir `\r\n` yra įvesties dalis ir duoda skirtingas maišas.
- **Dydžio ribos:**
  - ranka įvedama viena eilutė;
  - failas nuskaitomas į atmintį, todėl riba – .NET masyvo dydis (apie 2 GB);
  - išbandyta nuo 0 iki 100 000 baitų.
- **Maišos ilgis:** visada 256 bitai = 64 didieji šešioliktainiai simboliai (MD5 – 32, SHA-1 – 40).
- **Pradiniai nuliai išsaugomi:** Igno naudoja formatą `X16`, HashaMasha – `Convert.ToHexString`. Tai patikrinta 2 eksperimente.

## 3. Algoritmai

### Igno v0.12

```text
a, b, c, d ← keturios fiksuotos 64 bitų konstantos
kiekvienam baitui v pozicijoje i:                     (visi veiksmai moduliu 2^64)
    x ← (v + i) · M_idx
    a ← a ⊕ ((x + b) · M_A) ⊕ d;          b ← rotl(b, 21)
    b ← (((b + c) ⊕ a) · M_B) ⊕ d;        d ← rotl(d, 27)
    c ← (((c ⊕ b) · M_C) + d) ⊕ a;        c ← rotl(c, 61)
    d ← (((d ⊕ c) + a) · M_D) ⊕ b;        a ← rotl(a, 73)
baigiamasis maišymas:
    a ← ((a ⊕ (b · M_B)) · M_A) ⊕ c
    d ← (((b + c) ⊕ a) · M_C) ⊕ c
    c ← ((c ⊕ d) · M_D) ⊕ a
    b ← (((d + b) ⊕ a) · M_A) ⊕ c
    d ← rotl(d, 67)
grąžinti hex(a) ‖ hex(b) ‖ hex(c) ‖ hex(d)
```

Sprendimų pagrindimas:

- **Nelyginiai daugikliai** daugybą daro grįžtamą moduliu 2^64 ir paskleidžia bitus į aukštesnes pozicijas.
- **Cikliniai postūmiai** grąžina pokyčius į žemuosius bitus.
- **Baito pozicija `i`** užtikrina, kad sukeistos tvarkos įvestys duotų skirtingas maišas.

### IgnoAI v0.2 (sukurta su DI)

```text
a, b, c, d ← tos pačios konstantos kaip Igno
kiekvienam pilnam 32 baitų blokui (x0 … x3 – little-endian 64 bitų žodžiai):
    a ← rotl((a ⊕ (x0 + d)) · M_A, 23);   b ← rotl((b ⊕ (x1 + a)) · M_B, 29)
    c ← rotl((c ⊕ (x2 + b)) · M_C, 41);   d ← rotl((d ⊕ (x3 + c)) · M_D, 13)
jei lieka baitų: likę baitai ‖ 0x80 ‖ 0x00… (iki 32 baitų) sumaišomi taip pat
L ← ilgis baitais:  a ⊕= L·M_A;  b ⊕= rotl(L,17)·M_B;  c ⊕= (¬L)·M_C;  d ⊕= rotl(L,41)·M_D
4 kartus:  a ← rotl((a⊕d)·M_A + b, 23);  b ← rotl((b⊕a)·M_B + c, 29)
           c ← rotl((c⊕b)·M_C + d, 41);  d ← rotl((d⊕c)·M_D + a, 13)
grąžinti hex(a) ‖ hex(b) ‖ hex(c) ‖ hex(d)
```

Įvestis apdorojama 32 baitų blokais, todėl funkcija greitesnė. Ilgis įmaišomas pabaigoje, po jo seka 4 baigiamieji raundai.

### HashaMasha v0.2ai

```text
a, b, c, d ← 243F6A8885A308D3, 13198A2E03707344, A4093822299F31D0, 082EFA98EC4E6C89   (π skaitmenys)
m ← pranešimas ‖ 0x80 ‖ 0x00… ‖ ilgis baitais (64 bitų big-endian), kad ilgis būtų 32 baitų kartotinis
kiekvienam 32 baitų blokui (w0 … w3 – big-endian 64 bitų žodžiai):
    a ⊕= w0;  b ⊕= w1;  c ⊕= w2;  d ⊕= w3
    raundui r = 0 … 31:
        a ← a ⊕ K[r]
        a ← rotl(a + b, 13) ⊕ c;   b ← rotl(b + c, 29) ⊕ d
        c ← rotl(c + d, 41) ⊕ a;   d ← rotl(d + a, 7)  ⊕ b
grąžinti a ‖ b ‖ c ‖ d kaip 32 big-endian baitus šešioliktaine forma
```

Sprendimų pagrindimas:

- **SHA-2 stiliaus papildymas su ilgiu** yra vienareikšmis.
- **ARX raundai** (sudėtis, ciklinis postūmis, XOR) yra greiti ir nenaudoja lentelių.
- **32 raundai** duoda maždaug 50 % lavinos efektą.
- **Konstantas galima patikrinti:** π skaitmenys ir pirminių skaičių kubinės šaknys (SHA-512 `K`).

`v0.1ai` neturėjo `K`, visuose žingsniuose naudojo 13 bitų postūmį ir kopijuodavo visą pranešimą.

## 4. Versijos

| Žymė | Komitas | Realizacija | DI |
|---|---|---|---|
| `v0.1` | `89fed1a` | Igno pirmoji versija | ne |
| `v0.11` | `037d027` | Igno savarankiškas patobulinimas | ne |
| `v0.12` | `d40c7b3` | Igno savarankiškas patobulinimas – **geriausia savarankiška versija** | ne |
| `v0.2` | `85155cf` | IgnoAI | taip |
| `v0.1ai` | `0caeb72` | HashaMasha pirmoji versija | taip |
| `v0.2ai` | `5f089e6` | HashaMasha su DI pakeitimais | taip |

- **Ignas** pradėjo be DI ir DI panaudojo tik galutiniame etape (`v0.2`). Geriausia savarankiška versija `v0.12` užfiksuota prieš šį etapą.
- **Aivaras** DI naudojo visose versijose.
- **Vienodos sąlygos:** visos versijos palygintos tais pačiais eksperimentais, su tomis pačiomis įvestimis ir pradine reikšme.

## 5. Sąlygos ir atkūrimas

| Sąlyga | Reikšmė |
|---|---|
| Generatoriaus pradinė reikšmė (seed) | `676767` (`Helpers.Seed`); kiekviena funkcija gauna naują `Random(seed)`, todėl visos maišo tas pačias įvestis |
| Įvesties abėcėlė | `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789` – 62 simboliai, kiekvienas po vieną ASCII baitą (`Helpers.Alphabet`) |
| Imtys | 5 eksp.: 100 000 porų kiekvienam ilgiui 10, 100, 500, 1 000 ir 833 struktūruotos įvestys; 6 eksp.: 100 000 porų (po 25 000 kiekvienam ilgiui); 7 eksp.: 10 000 kandidatų |
| Kalba ir kompiliatorius | C# 14, .NET 10, kompiliuota Release konfigūracija |
| Vykdymo aplinka – 4 ir 6 eksp. | 13th Gen Intel(R) Core(TM) i7-13650HX, 2600 MHz, 14 branduolių, 20 loginių procesorių; 23,7 GB RAM; Windows 11 |
| Vykdymo aplinka – kiti eksperimentai | Apple M3 Pro, macOS 26.1, .NET SDK 10.0.203, Release konfigūracija, 2026-09-30 |
| Pradiniai matavimai | `Hash.Experiments/Results/Experiment<N>_<funkcija>.csv` |

2, 3, 5 ir 6 eksperimentų rezultatai deterministiniai, todėl juos galima atkurti bet kuriame kompiuteryje. Nuo kompiuterio priklauso tik laikai.

## 6. Eksperimentų rezultatai

### 1. Įvestys

| Atvejis | Failas | Baitai | Igno v0.12 | IgnoAI v0.2 | Aivaro v0.2ai |
|---|---|---:|---|---|---|
| Tuščia | EmptyFile.txt | 0 | `677035964B6F…` | `C842B1306428…` | `2B6AEC85F479…` |
| Vieno baito | OneByteFile.txt | 1 | `3A27DAFBA132…` | `F8E102B39BF1…` | `C90867AF2AEA…` |
| Atsitiktinė > 1 000 B | RandomLatinOver1000Bytes.txt | 4 104 | `9BBC9C32A5D9…` | `D0FBB9D07BEA…` | `F0B25592840C…` |
| Pakeistas pirmas baitas | RandomLatinOver1000BytesChangedFront.txt | 4 104 | `985C834C9ECE…` | `D8C4384F6575…` | `AE81E71950BF…` |
| Atsitiktinė > 1 000 B | SecondRandomLatinOver1000Bytes.txt | 3 788 | `EDCF8ECAB4B6…` | `0228AD8B41C8…` | `74B586F4071F…` |
| Pakeistas paskutinis baitas | SecondRandomLatinOver1000BytesChangedBack.txt | 3 788 | `781D0800383F…` | `BB42427B7F2F…` | `A1325369E484…` |
| Struktūruota | StructuralFile.txt (`opopopop`) | 8 | `40E855378F4A…` | `E0AE7D7E1969…` | `2566AF5CD0F2…` |
| Sukeista tvarka | StructuralFileChangedOrder.txt (`popopopo`) | 8 | `05F660C79344…` | `EE3DBD9FD295…` | `4C734CAB4C83…` |
| Eilutės pabaiga | StructuralFileNewline.txt | 9 | `A6843C9E400B…` | `D681192434D6…` | `79D37CD484F0…` |
| Pradiniai tarpai | StructuralFileFourSpacePadding.txt | 12 | `6A20D19C4048…` | `4FF678F28071…` | `18565FABE6EB…` |
| UTF-8 su ne ASCII (lenkų k.) | UTF8.txt | 314 | `8A0341EA7D3B…` | `94C7FEF3E8EB…` | `E12AE059D18E…` |
| UTF-8 su ne ASCII (lietuvių k.) | konstitucija.txt | 75 595 | `876FF9AE6753…` | `62C842ED7D63…` | `0E29D235CB8E…` |

Rodomi pirmieji 12 iš 64 simbolių. Vieno baito pakeitimas ar kita tvarka duoda visiškai nesusijusią maišą.

### 2. Formatas

- **Ilgis ir simboliai:** visos maišos buvo deklaruoto ilgio ir sudarytos tik iš `0–9A–F`.
- **Pradiniai nuliai:** maiša, prasidedanti `0`, rasta kiekvienai funkcijai (Igno po 25 bandymų, IgnoAI – po 19, Aivaro – po 3) ir išspausdinta viso ilgio.
- **Ranka įvesta ir failo įvestis:** `ABC` duoda tą pačią maišą kaip `Experiment2.txt`, bet tik pašalinus iš failo UTF-8 BOM.

### 3. Determinizmas

Visų trijų funkcijų rezultatai nuoseklūs šiuose patikrinimuose:

- ta pati įvestis 5 kartus iš eilės;
- seka A, B, A kiekvienai įvesčių porai;
- 3 atskiri procesai;
- palyginimas su ankstesniu paleidimu.

### 4. Sparta

Įvestys – pirmosios 1, 2, 4, 8, … `konstitucija.txt` eilutės ir visas failas. Laikas neapima I/O. Kiekvienam dydžiui atliekami 5 įšildymo kvietimai ir 5 matavimai po 1 000 kvietimų. Lentelėse – vienos maišos laiko vidurkis µs, skliaustuose – mažiausias ir didžiausias matavimas.

| Baitai | Igno v0.12 | IgnoAI v0.2 | Aivaro v0.2ai |
|---:|---:|---:|---:|
| 70 | 0,55 (0,52–0,57) | 0,18 (0,18–0,19) | 1,52 (1,46–1,57) |
| 123 | 0,83 (0,82–0,86) | 0,24 (0,21–0,33) | 1,96 (1,82–2,26) |
| 205 | 1,24 (1,20–1,26) | 0,32 (0,30–0,38) | 2,16 (1,61–2,49) |
| 362 | 2,14 (2,04–2,22) | 0,43 (0,41–0,45) | 2,57 (2,38–2,78) |
| 996 | 6,91 (5,55–7,95) | 0,97 (0,95–1,00) | 1,96 (1,16–4,91) |
| 1 841 | 9,25 (8,98–9,45) | 1,65 (1,64–1,67) | 2,18 (2,08–2,41) |
| 3 712 | 17,23 (16,43–18,77) | 3,24 (3,20–3,30) | 4,30 (4,16–4,55) |
| 9 155 | 38,54 (36,81–40,28) | 7,82 (7,74–8,03) | 10,49 (10,14–10,86) |
| 20 409 | 83,45 (82,55–84,56) | 17,44 (17,15–17,84) | 23,14 (22,77–23,77) |
| 47 434 | 193,03 (190,04–195,93) | 34,91 (7,40–42,16) | 54,36 (52,84–56,67) |
| 75 595 | 304,90 (301,87–308,04) | 12,04 (11,95–12,18) | 87,87 (85,99–89,08) |

| Baitai | MD5 | SHA-1 | SHA-256 |
|---:|---:|---:|---:|
| 70 | 0,36 (0,31–0,49) | 0,32 (0,30–0,37) | 0,25 (0,21–0,30) |
| 123 | 0,42 (0,39–0,47) | 0,37 (0,37–0,38) | 0,25 (0,24–0,26) |
| 205 | 0,49 (0,49–0,51) | 0,47 (0,45–0,49) | 0,28 (0,27–0,29) |
| 362 | 0,66 (0,65–0,66) | 0,59 (0,57–0,66) | 0,96 (0,32–3,34) |
| 996 | 1,54 (1,53–1,55) | 1,28 (1,25–1,34) | 0,66 (0,62–0,68) |
| 1 841 | 2,68 (2,66–2,75) | 2,26 (2,14–2,42) | 1,15 (1,00–1,28) |
| 3 712 | 5,40 (5,30–5,61) | 4,34 (4,28–4,49) | 1,86 (1,76–1,99) |
| 9 155 | 12,87 (12,75–13,09) | 10,17 (9,97–10,43) | 4,13 (4,04–4,31) |
| 20 409 | 29,10 (28,51–29,91) | 22,47 (21,89–23,10) | 9,05 (8,91–9,19) |
| 47 434 | 66,20 (65,79–66,81) | 51,57 (50,97–52,19) | 20,82 (20,65–20,94) |
| 75 595 | 105,60 (104,98–105,97) | 82,51 (81,07–84,66) | 33,05 (32,68–33,22) |

<img width="942" height="546" alt="image" src="https://github.com/user-attachments/assets/1793daa8-22e7-4756-8dda-d06c8506c6fe" />

- **Palyginimas:**
  - Igno maišo po vieną baitą ir yra apie 3,5 karto lėtesnė už Aivaro.
  - IgnoAI, esant 3–20 KB, yra apie 1,3 karto greitesnė už Aivaro.
  - Aivaro veikia panašiai kaip SHA-1 ir greičiau nei MD5.
- **Nenuoseklūs matavimai:**
  - IgnoAI ties 47 434 B svyravo nuo 7,40 iki 42,16 µs, o ties 75 595 B užtruko trumpiau nei ties 20 409 B. Tikėtina priežastis – matavimo metu įsijungęs .NET kodo optimizavimas (JIT).
  - Mažoms įvestims laikai taip pat svyruoja.

### 5. Kolizijos

| Įvesčių rinkinys | Patikrinta porų | Kolizijos porose | Skirtingos įvestys | Skirtingų įvesčių grupės su bendra maiša |
|---|---:|---:|---:|---:|
| Atsitiktinės, ilgis 10 | 100 000 | 0 | 200 000 | 0 |
| Atsitiktinės, ilgis 100 | 100 000 | 0 | 200 000 | 0 |
| Atsitiktinės, ilgis 500 | 100 000 | 0 | 200 000 | 0 |
| Atsitiktinės, ilgis 1 000 | 100 000 | 0 | 200 000 | 0 |
| Struktūruotos (perstatymai, pasikartojimai, bitų apvertimai, papildymą primenančios) | – | – | 831 iš 833 | 0 |

- **Tikrinimo būdas:** kiekvienos poros įvestys skiriasi, o visas kiekvieno ilgio rinkinys papildomai patikrintas dėl bendrų maišų. Kolizija skaičiuojama tik skirtingoms įvestims.
- **Rezultatas:** toks pat visoms versijoms (`v0.1`–`v0.2ai`) ir MD5, SHA-1, SHA-256.

### 6. Lavinos efektas

100 000 porų, po 25 000 kiekvienam ilgiui 10, 100, 500 ir 1 000. Kiekvienoje poroje pakeistas vienas simbolis. Bitai lyginami dekodavus šešioliktainę maišą. Reikšmės pateikiamos procentais nuo visų išvesties bitų arba šešioliktainių simbolių.

| Funkcija | Ilgis | Bitų min. | Bitų maks. | Bitų vid. | Hex min. | Hex maks. | Hex vid. |
|---|---:|---:|---:|---:|---:|---:|---:|
| Igno v0.12 | 10 | 37,50 | 63,28 | 50,01 | 79,69 | 100 | 93,75 |
| | 100 | 38,28 | 62,50 | 49,98 | 78,13 | 100 | 93,72 |
| | 500 | 37,50 | 60,94 | 49,99 | 78,13 | 100 | 93,72 |
| | 1 000 | 36,72 | 63,67 | 50,01 | 78,13 | 100 | 93,76 |
| | **iš viso** | **36,72** | **63,67** | **50,00** | **78,13** | **100** | **93,74** |
| IgnoAI v0.2 | 10 | 37,11 | 62,89 | 50,00 | 79,69 | 100 | 93,74 |
| | 100 | 37,11 | 61,72 | 50,03 | 78,13 | 100 | 93,78 |
| | 500 | 38,28 | 62,11 | 50,01 | 78,13 | 100 | 93,74 |
| | 1 000 | 38,28 | 62,11 | 50,00 | 75,00 | 100 | 93,76 |
| | **iš viso** | **37,11** | **62,89** | **50,01** | **75,00** | **100** | **93,76** |
| Aivaro v0.2ai | 10 | 37,11 | 64,06 | 49,99 | 81,25 | 100 | 93,73 |
| | 100 | 37,50 | 62,11 | 49,99 | 79,69 | 100 | 93,78 |
| | 500 | 37,89 | 62,11 | 50,03 | 79,69 | 100 | 93,77 |
| | 1 000 | 37,50 | 64,45 | 50,02 | 78,13 | 100 | 93,77 |
| | **iš viso** | **37,11** | **64,45** | **50,01** | **78,13** | **100** | **93,76** |
| MD5 | 10 | 32,81 | 66,41 | 50,04 | 71,88 | 100 | 93,77 |
| | 100 | 32,81 | 69,53 | 50,04 | 68,75 | 100 | 93,80 |
| | 500 | 29,69 | 67,19 | 50,03 | 68,75 | 100 | 93,80 |
| | 1 000 | 31,25 | 68,75 | 50,03 | 68,75 | 100 | 93,79 |
| | **iš viso** | **29,69** | **69,53** | **50,04** | **68,75** | **100** | **93,79** |
| SHA-1 | 10 | 32,50 | 65,00 | 50,05 | 75,00 | 100 | 93,81 |
| | 100 | 34,38 | 66,88 | 50,03 | 72,50 | 100 | 93,79 |
| | 500 | 34,38 | 65,00 | 50,00 | 72,50 | 100 | 93,75 |
| | 1 000 | 32,50 | 65,63 | 49,97 | 75,00 | 100 | 93,75 |
| | **iš viso** | **32,50** | **66,88** | **50,01** | **72,50** | **100** | **93,78** |
| SHA-256 | 10 | 37,89 | 62,11 | 50,00 | 78,13 | 100 | 93,74 |
| | 100 | 38,67 | 61,33 | 50,00 | 78,13 | 100 | 93,72 |
| | 500 | 37,50 | 61,72 | 50,03 | 78,13 | 100 | 93,78 |
| | 1 000 | 37,50 | 62,11 | 50,00 | 78,13 | 100 | 93,73 |
| | **iš viso** | **37,50** | **62,11** | **50,01** | **78,13** | **100** | **93,74** |

Bitų skirtumo histograma – porų skaičius intervale (0–25 % ir 70–100 % porų nėra):

| Pakitę bitai | Igno v0.12 | IgnoAI v0.2 | Aivaro v0.2ai | MD5 | SHA-1 | SHA-256 |
|---|---:|---:|---:|---:|---:|---:|
| 25–30 % | 0 | 0 | 0 | 1 | 0 | 0 |
| 30–35 % | 0 | 0 | 0 | 27 | 6 | 0 |
| 35–40 % | 94 | 82 | 62 | 1 312 | 451 | 58 |
| 40–45 % | 5 843 | 5 758 | 5 721 | 11 004 | 8 539 | 5 800 |
| 45–50 % | 41 702 | 41 514 | 41 716 | 33 787 | 37 705 | 41 660 |
| 50–55 % | 46 346 | 46 684 | 46 537 | 41 209 | 41 414 | 46 528 |
| 55–60 % | 5 946 | 5 895 | 5 888 | 11 298 | 11 179 | 5 897 |
| 60–65 % | 69 | 67 | 76 | 1 327 | 697 | 57 |
| 65–70 % | 0 | 0 | 0 | 35 | 9 | 0 |

<img width="752" height="452" alt="image" src="https://github.com/user-attachments/assets/5f23ad5c-f50e-446e-907b-593dc5e1ecba" />


- **256 bitų funkcijos:** visos trys mūsų funkcijos ir SHA-256 elgiasi vienodai. Vidurkis yra apie 50 %, o 99,84–99,89 % porų pakeitė 40–60 % bitų.
- **MD5 ir SHA-1:** pasiskirstymas platesnis (97,30 % ir 98,84 % porų intervale 40–60 %), nes jų išvestis trumpesnė.

### 7. Spėjimas, vieša druska ir slaptas r

Kandidatai – eilutės nuo `0000` iki `9999`. Tikslinė įvestis `9388` atakos procedūrai neperduodama. Tikrinami visi kandidatai ir fiksuojami visi sutapimai.

- **Druska:** 16 atsitiktinių baitų, prijungiamų po 4 ASCII skaitmenų ir rodomų kaip 32 šešioliktainiai simboliai.
- **Viena druska vienam taikiniui:** vienam taikiniui druska fiksuota, o kiekvienas iš 5 kitų taikinių turi atskirą druską.

Laikai išmatuoti Apple M3 Pro kompiuteryje:

| Scenarijus | Bandymai | Rezultatas | Igno v0.12 | IgnoAI v0.2 | Aivaro v0.2ai |
|---|---:|---|---:|---:|---:|
| Be druskos, `H(input)` | 10 000 | vienintelis sutapimas `9388` | 7,5 ms | 2,5 ms | 0,9 ms |
| Vieša druska, `H(input ‖ salt)` | 10 000 | vienintelis sutapimas `9388` | 3,0 ms | 2,5 ms | 0,7 ms |
| Viena lentelė be druskos, 5 taikiniai | 10 000 | rasti 5 iš 5 | 4,2 ms | 4,1 ms | 3,3 ms |
| Pirmosios druskos lentelė, 5 taikiniai su skirtingomis druskomis | 10 000 | rastas 1 iš 5 | 3,3 ms | 2,8 ms | 1,1 ms |
| Atskira paieška kiekvienai druskai | 50 000 | rasti 5 iš 5 | 22,8 ms | 14,0 ms | 6,2 ms |
| Slaptas 1 baito `r` (visos poros) | 2 560 000 | rasti `9388` ir `r` | 550 ms | 621 ms | 177 ms |
| Slaptas 16 baitų `r` (įvertis, nevykdyta) | 3,4 · 10⁴² | – | ~2 · 10²⁸ m. | ~3 · 10²⁸ m. | ~8 · 10²⁷ m. |
| `r` atskleistas | 10 000 | rasta `9388` | 2,9 ms | 2,5 ms | 0,7 ms |

- **Vienas taikinys:** druska nesulėtina vieno taikinio paieškos.
- **Keli taikiniai:** be druskos viena lentelė tinka visiems taikiniams, o su skirtingomis druskomis kiekvienam reikia atskiros paieškos.
- **Slaptas `r`:** kol `r` nežinomas, paieškos erdvė yra 10 000 · 2¹²⁸. Atskleidus `r`, įvestį galima patikrinti arba atspėti per 10 000 bandymų.

## 7. Išvados ir versijų palyginimas

| | Igno `v0.1` | Igno `v0.11` | Igno `v0.12` | IgnoAI `v0.2` | Aivaro `v0.1ai` | Aivaro `v0.2ai` |
|---|---|---|---|---|---|---|
| DI | ne | ne | ne | taip | taip | taip |
| Determinizmas (3 eksp.) | taip | taip | taip | taip | taip | taip |
| Kolizijos (5 eksp.) | 0 | 0 | 0 | 0 | 0 | 0 |
| Lavinos efekto vidurkis (6 eksp.) | 49,08 % | 49,08 % | 50,00 % | 50,01 % | 50,00 % | 50,01 % |
| Žemieji išvesties bitai nereaguoja į aukščiausią įvesties baito bitą | taip | taip | ne | ne | ne | ne |
| Išvesties bito šališkumas | nėra | **63 bitas = 1 tik 25 % atvejų** | nėra | nėra | nėra | nėra |
| Akimirksniu sukonstruojama kolizija ir pirmavaizdis | nerasta | nerasta | nerasta | **taip** | **taip** | **taip** |

**Pagerėjimai**

- **Igno `v0.12`:** cikliniai postūmiai pradėjo veikti, šališkumas dingo, o lavinos efektas pakilo nuo 49,08 % iki 50,00 %.
- **IgnoAI `v0.2`:** esant 3–20 KB, apie 5 kartus greitesnė už Igno.
- **Aivaro `v0.2ai`:**
  - kiekviename raunde skirtinga konstanta;
  - skirtingi postūmiai;
  - nebėra nulinės būsenos, kuri raunduose lieka nulinė;
  - pranešimas nebekopijuojamas.

**Pablogėjimai**

- **Igno `v0.11`:** baigiamasis maišymas daugino iš būsenos žodžio `c`, todėl 63 išvesties bitas buvo lygus 1 tik 25 % atvejų. Be to, `RotateLeft` rezultatai nebuvo priskirti. Abi problemos ištaisytos `v0.12`.
- **IgnoAI `v0.2`:** greitis pasiektas saugumo sąskaita.

**Silpnybės**

- **HashaMasha (`v0.1ai`, `v0.2ai`) ir IgnoAI (`v0.2`):**
  - *Priežastis:* bloko įmaišymą galima išspręsti atgal. HashaMasha blokas XOR operacija sujungiamas su visa būsena, o raundus galima vykdyti atbuline tvarka. IgnoAI blokas įmaišomas vienu grįžtamu žingsniu.
  - *Atakos:* su tikra realizacija akimirksniu sukonstravome koliziją (`Alice pays Bob 1 coin…` ir `Alice pays Eve 9999 coins…` su apskaičiuotu antruoju bloku), pirmavaizdį bet kokiai maišai ir ilgio išplėtimą (`secret ‖ papildymas ‖ priedas` maišą nežinant paslapties).
  - *Sprendimas:* kempinės (sponge) konstrukcija.
- **Igno `v0.1` ir `v0.11`:** pokyčiai sklido tik aukštyn, todėl aukščiausio baito bito pakeitimas nepakeisdavo žemiausių 7 kiekvieno išvesties žodžio bitų (10 000 iš 10 000 bandymų). Ištaisyta `v0.12`.
- **Igno `v0.12`:** struktūrinės atakos neradome, bet jos ir išsamiai netyrėme.

**Ko testai neįrodo**

- **Atsparumo kolizijoms.** Atsitiktinė 256 bitų kolizija tikėtina tik po maždaug 2¹²⁸ bandymų, o mes palyginome apie 8 · 10¹⁰ porų. Nulį kolizijų surinko ir MD5 bei SHA-1, kurioms žinomos atakos, ir HashaMasha bei IgnoAI.
- **Atsparumo pirmavaizdžiams.** 7 eksperimentas rodo tik tai, kad mažos aibės įvestį galima atspėti.
- **Saugumo iš gero lavinos efekto.** HashaMasha ir IgnoAI lavinos efektas yra 50,01 %, bet abi pažeidžiamos.
- **Bendrų greičio teiginių.** Laikai priklauso nuo kompiuterio ir .NET optimizavimo.

**Ryšys su paskaitos sąvokomis**

- **Pirmavaizdžiai:** juos sunku rasti tik tada, kai įvesčių erdvė didelė *ir* funkcijos negalima vykdyti atgal.
- **Kolizijos:** atsitiktinė paieška randa tik dažnas kolizijas, o tikras atsparumas reiškia atsparumą ir sukonstruotoms kolizijoms.
- **Lavinos efektas:** būtinas, bet nepakankamas. Igno `v0.11` problemą parodė tik atskirų bitų tikrinimas.
- **Patikimos maišos reikšmės:** maiša turi būti deterministinė, su fiksuotu kodavimu ir ilgiu (mūsų funkcijos tai tenkina) bei atspari kolizijoms ir pirmavaizdžiams. HashaMasha ir IgnoAI šio reikalavimo netenkina, o Igno `v0.12` atsparumas neįrodytas. Realiam naudojimui – SHA-256.

## 8. Indėlis

- **Ignas Šimaitis:**
  - Igno (`v0.1`–`v0.12`) ir IgnoAI (`v0.2`)
  - `InputReader`
  - vienetų testai
  - realizacijų sujungimas
  - eksperimentų paleidimo šablonas
  - 2, 4 ir 6 eksperimentai
  - atsitiktinių eilučių pagalbinė funkcija
  - MD5, SHA-1 ir SHA-256 palyginimui
- **Aivaras Jomantas:**
  - HashaMasha (`v0.1ai`, `v0.2ai`)
  - komandinės eilutės meniu ir įvesties logika
  - 3, 5 ir 7 eksperimentai
  - 2 eksperimento testiniai failai
  - README

## 9. Šaltiniai ir DI naudojimas

**Šaltiniai**

- **π šešioliktainiai skaitmenys** (kaip BLAKE-512 ir Blowfish) – HashaMasha pradinės reikšmės.
- **NIST FIPS 180-4** – SHA-512 raundų konstantos ir SHA-2 papildymo schema.
- **StackOverflow** – hex formato tikrinimo reguliarioji išraiška (`Helpers.AssertValidHash`).
- **.NET `System.Security.Cryptography`** – MD5, SHA-1 ir SHA-256, naudojamos tik palyginimui.

**Aivaras:** Claude (Anthropic), modelis Claude Opus 5.5, per Claude Code (VS Code), 2026-09-30.

| DI pasiūlymas | Sprendimas | Patikra |
|---|---|---|
| 3 eksp.: atskiri atskaitos failai kiekvienai funkcijai, bendra pradinė reikšmė, patikrinimas su atskirais procesais | priimta | paleista; tyčia kiekviename procese kitokią maišą duodanti funkcija aptikta |
| 2, 5 ir 7 eksperimentų realizacijos ir pakeitimai | priimta | sukompiliuota ir paleista visoms funkcijoms |
| HashaMasha kolizijos ir pirmavaizdžio ataka | priimta kaip rasta silpnybė | patikrinta su tikra realizacija |
| HashaMasha perdaryti į kempinės konstrukciją | **atmesta** – išlaikyta sava 4 žodžių struktūra | – |
| HashaMasha `v0.2ai`: raundų konstantos, skirtingi postūmiai, pranešimo nekopijavimas | priimta | 1 504 pranešimų maišos sutapo su senuoju papildymo būdu; lavinos efektas nepakito |
| CSV formatavimas su `CultureInfo.InvariantCulture` | neįgyvendinta | – |
| README struktūra, lentelės, grafikai ir vertimas | priimta | skaičiai sutikrinti su pateiktomis lentelėmis |

**Ignas:** IgnoAI (`v0.2`, `HashAI.cs`) sukurta su DI pagalba.
