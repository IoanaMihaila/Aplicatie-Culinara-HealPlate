# 🍽️ HealPlate

**HealPlate** este o aplicație web dezvoltată în ASP.NET Core Razor Pages, creată pentru a sprijini persoanele cu restricții alimentare medicale. Oferă rețete personalizate, planuri nutriționale, chatbot AI, statistici și funcții moderne precum scanarea ingredientelor din imagini.

---

## 🚀 Funcționalități principale

- 🍽️ Vizualizarea rețetelor pe categorii și căutarea lor rapidă
- 🧑‍🍳 Instrucțiuni audio pentru modul de preparare
- 📝 Recenzii vocale pentru rețete
- 🛒 Adăugare ingrediente în coșul de cumpărături și link către magazine online
- 📌 Colectarea rețetelor preferate într-un spațiu personalizat
- ➕ Adăugare de rețete proprii
- 📷 Scanare etichete de produse cu verificarea automată a alergenilor
- 📆 Generare plan alimentar personalizat fără alergeni
- 🧾 Vizualizarea planurilor alimentare zilnice
- 🔥 Calcul automat al necesarului caloric zilnic
- 📊 Statistici nutriționale personalizate (retete/ingrediente/timp gătit)
- 📍 Căutare pe hartă a restaurantelor & magazinelor bio care exclud alergenii (în Timișoara)
- 🧠 Recunoaștere ingrediente din imagini și sugestie rețete pe baza lor
- 🧩 Preluare ingrediente de la o extensie browser și căutare rețete pe baza lor

---

## 🛠️ Tehnologii utilizate

- ASP.NET Core Razor Pages  
- Entity Framework Core (EF Core)  
- SQL Server / LocalDB  
- C#  
- Google Vision API  
- Chart.js  
- JavaScript, HTML, CSS  
- Bootstrap  

---

## ▶️ Cum rulezi aplicația local

### 1. Clonează proiectul

git clone https://github.com/NumeleTau/HealPlate.git
cd HealPlate

### 2. Configurează baza de date

Se va folosi baza de date HealPlateDb din SQL Server Management Studio și se va actualiza, după caz, string-ul de conexiune la baza de date în fișierul appsettings.json

### 3. Aplică migrările Entity Framework Core

Scaffold-DbContext "Name=DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context HealPlateDbContext -Force

### 4. Rulează aplicația

În Visual Studio, selectează profilul https sau IIS Express, apasă F5 sau CTRL+F5

### 5. Capturi de ecran

![image](https://github.com/user-attachments/assets/e6094100-74db-41d8-81ed-464929e2a02e)
![image](https://github.com/user-attachments/assets/3c80d764-49c4-468b-bca9-8c66b275bc11)
![image](https://github.com/user-attachments/assets/31b46e49-f3f1-4a5e-9f25-6cc851085575)
![image](https://github.com/user-attachments/assets/d1a38f52-2bb7-49b5-a391-53cb547a9d83)
![image](https://github.com/user-attachments/assets/17e113ac-efb1-4b55-83af-202c2ba9e220)
![image](https://github.com/user-attachments/assets/8cc9e441-dd2c-438f-947f-c597b7bb23a3)
![image](https://github.com/user-attachments/assets/fc425077-1f7f-409d-92dd-299003474857)
![image](https://github.com/user-attachments/assets/1338ffaf-2c9e-4ffd-bc2e-8c554968c84c)
![image](https://github.com/user-attachments/assets/ad85c22f-9f38-410b-95cb-a081a34a1a91)
![image](https://github.com/user-attachments/assets/6cd61c2f-d703-4a85-a9fb-66dfb8b0ffc4)

### 6. Funcționalități cheie

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

### 7. Testarea aplicației

Pentru a asigura fiabilitatea și corectitudinea funcționalităților centrale din aplicația HealPlate, am implementat un set cuprinzător de teste unitare, utilizând xUnit în combinație cu furnizorul InMemory al Entity Framework Core. Această abordare mi-a permis să simulez un mediu de baze de date fără a afecta datele reale stocate.
