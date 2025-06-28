# HEALPLATE APLICAȚIE WEB CULINARĂ PENTRU PERSOANELE CU INTOLERANȚE ALIMENTARE MEDICALE

**HealPlate** este o aplicație web dezvoltată în ASP.NET Core Razor Pages, creată pentru a sprijini persoanele cu restricții alimentare medicale. Oferă rețete personalizate, planuri nutriționale, chatbot AI, statistici și funcții moderne precum scanarea ingredientelor din imagini.

---

##  Adresă repository

Codul sursă complet este disponibil în repository-ul GitHub:

 [https://github.com/IoanaMihaila/Aplicatie-Culinara-HealPlate](https://github.com/IoanaMihaila/Aplicatie-Culinara-HealPlate)

---

##  Tehnologii utilizate

- ASP.NET Core Razor Pages  
- Entity Framework Core 
- Miscrosoft SQL Server Studio
- C#  
- Google Vision API
- Google Maps API
- Ollama, Mistral
- Chart.js  
- JavaScript, HTML, CSS  
- Bootstrap  

---

##  Cum rulezi aplicația local

### 1. Clonează proiectul

git clone https://github.com/IoanaMihaila/Aplicatie-Culinara-HealPlate.git

### 2. Instalare .NET

https://dotnet.microsoft.com/download

### 3. Configurează baza de date

Se va folosi baza de date HealPlateDb din SQL Server și se va actualiza, după caz, string-ul de conexiune la baza de date în fișierul appsettings.json

### 4. Aplică migrările Entity Framework Core

Pentru a crea baza de date și toate tabelele necesare pe baza modelelor C#, urmează acești pași:
4.1. Deschide un terminal în directorul proiectului
4.2. Creează o migrare rulând comanda:
     dotnet ef database update

### 5. Rulează aplicația

În Visual Studio, selectează profilul https sau IIS Express, apasă F5 sau CTRL+F5 sau rulează comanda dotnet run

### Funcționalități cheie

Utilizator:

- Înregistrare cu verificare email si stabilire alergeni
- Autentificare
- Vizualizare rețete personalizate pe categorii, care exclud alergenii selectați
- Căutare rețete dupa denumire sau ingrediente
- Vizualizare detalii rețete cu suport audio
- Adăugare(cu suport vocal), editare, ștergere recenzie pentru o rețetă
- Adăugare ingredient în coșul de cumpărături
- Salvare rețete în colecția personală
- Adăugare rețetă nouă cu trimiterea unei notificări către admin pentru aprobarea postării
- Scanare etichetă produs pentru verificarea ingredientelor periculoase
- Localizare magazine/restaurante bio care comercializează produse care exclud alergenii utilizatorului pe o anumită zonă
- Calcularea necesarului zilnic de calorii
- Generarea unui plan alimentar, salvarea în calendar și trimiterea unui reminder pe email
- Test nutritional pentru detectarea altor posibile intoleranțe
- Editare restricții culinare în secțiunea ‘Profil personal’
- Conversare cu chatbot asistent nutrițional
- Recunoaștere ingrediente din imagini și sugestie rețete pe baza lor
- Preluare ingrediente de la o extensie browser și căutare rețete pe baza lor

Admin:
- Editare și ștergere rețete
- Editare întrebări chestionar
- Statistici despre planurile alimentare salvate, rețete, utilizatori și alergeni frecvent întâlniți
- Aprobare sau respingere postare pentru o rețetă adăugată de utilizator și trimitere notificare pentru anunțare
- Descărcare raport planuri nutriționale în format PDF

### Testarea aplicației

Pentru a asigura fiabilitatea și corectitudinea funcționalităților centrale din aplicația HealPlate, am implementat un set cuprinzător de teste unitare, utilizând xUnit în combinație cu furnizorul InMemory al Entity Framework Core. Această abordare mi-a permis să simulez un mediu de baze de date fără a afecta datele reale stocate.
