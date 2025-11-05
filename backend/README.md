# Diff Spectrum View - Backend API

Backend מקצועי ב-C# .NET 6.0 לניהול והשוואת תגובות API בין סביבות Production ו-Integration.

## 🏗️ ארכיטקטורה

הפרויקט בנוי לפי עקרונות **Clean Architecture** ו-**SOLID**:

\`\`\`
backend/
├── Controllers/        # API Endpoints
├── Services/          # Business Logic
├── Repositories/      # Data Access Layer
├── Models/           # Domain Entities
├── DTOs/             # Data Transfer Objects
├── Database/         # SQL Scripts
└── Program.cs        # Application Entry Point
\`\`\`

## 📦 ספריות

הפרויקט משתמש **רק** בספריות הבאות:

- **Hangfire 1.8.14** - Background jobs
- **Hangfire.AspNetCore 1.8.14** - Hangfire integration
- **Hangfire.Core 1.8.14** - Core functionality
- **Hangfire.SqlServer 1.8.14** - SQL Server storage
- **System.Data.SqlClient 4.8.6** - Database access
- **Swashbuckle.AspNetCore 6.2.3** - API documentation

## 🚀 התקנה והרצה

### דרישות מקדימות
- .NET 6.0 SDK
- SQL Server (Local או Remote)

### שלבי התקנה

1. **שכפול הפרויקט**
\`\`\`bash
cd backend
\`\`\`

2. **עדכון Connection String**
ערוך את `appsettings.json`:
\`\`\`json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=DiffSpectrumView;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
\`\`\`

3. **יצירת Database**
הרץ את הסקריפטים מתיקיית `Database/`:
- `01_CreateTables.sql` - יוצר טבלאות
- `02_SeedData.sql` - מוסיף נתוני דוגמה

4. **הרצת הפרויקט**
\`\`\`bash
dotnet restore
dotnet run
\`\`\`

הAPI יהיה זמין ב: `https://localhost:5001`

## 📚 API Endpoints

### Comparison (Flapi Integration)
- `POST /api/comparison` - קבלת בקשות מ-Flapi להשוואה אסינכרונית
  - Body: `DuplicationRequest` (testUrl, sourceUrl, content, expectedResponse, options)
  - מחזיר: `202 Accepted` (הבקשה נשלחת ל-Hangfire לעיבוד)

### Diffs
- `GET /api/diffs` - קבלת כל ההבדלים
- `GET /api/diffs/{id}` - קבלת הבדל לפי ID
- `GET /api/diffs/job/{jobId}` - קבלת הבדלים לפי Job
- `DELETE /api/diffs/{id}` - מחיקת הבדל (soft delete)
- `POST /api/diffs/{id}/restore` - שחזור הבדל
- `PATCH /api/diffs/{id}/checked` - עדכון סטטוס checked

### Jobs
- `GET /api/jobs` - קבלת כל ה-Jobs
- `GET /api/jobs/{id}` - קבלת Job לפי ID
- `GET /api/jobs/summary` - קבלת סיכום Jobs

### Hangfire Dashboard
- `/hangfire` - ממשק ניהול Background Jobs

## 🔄 איך זה עובד?

### תהליך השוואה (Flapi Integration)

1. **Flapi שולח בקשה** ל-`POST /api/comparison` עם:
   - `testUrl` - URL של הסביבה הנבדקת
   - `sourceUrl` - URL של Flapi (מקור)
   - `content` - תוכן הבקשה (JSON)
   - `expectedResponse` - התשובה הצפויה מ-Flapi

2. **Backend מקבל ומעבד**:
   - מקבל את הבקשה ומחזיר `202 Accepted` מיד (fire and forget)
   - שולח את הבקשה ל-Hangfire לעיבוד אסינכרוני
   - Hangfire שולח את הבקשה לסביבה הנבדקת
   - משווה בין התשובה מ-Flapi לתשובה מהסביבה הנבדקת

3. **השוואה**:
   - משווה **רק Body ו-Status Code** (לא Headers!)
   - מנרמל את התשובות לפני השוואה
   - מזהה הבדלים (JSON Response, Status Code)

4. **שמירה ב-DB**:
   - Job נשמר כ-"Completed" **בין אם יש הבדלים ובין אם לא**
   - Diff נשמר **רק אם יש הבדלים בפועל**
   - כל Diff כולל: SourceRequest, TargetRequest, NormalizedResponses, CompleteResponses

### הבדל בין Job ל-Diff

- **Job** = הרצת Hangfire (הצלחה/כישלון של התהליך)
  - Job מצליח = הבקשה נשלחה, התקבלה תשובה, וההשוואה בוצעה
  - Job נכשל = שגיאה בתהליך (timeout, connection error, וכו')

- **Diff** = הבדל בפועל בין התשובות
  - נשמר רק כשיש הבדל בין source ל-target
  - יכול להיות Job מוצלח ללא Diffs (כשהתשובות זהות)

## 🎯 תכונות

✅ **Repository Pattern** - הפרדה בין Business Logic ל-Data Access  
✅ **Dependency Injection** - ניהול תלויות נקי  
✅ **Soft Delete** - מחיקה לוגית של נתונים  
✅ **Background Jobs** - עיבוד אסינכרוני עם Hangfire  
✅ **Swagger Documentation** - תיעוד API אוטומטי  
✅ **CORS Support** - תמיכה בקריאות מהקליינט  
✅ **Flapi Integration** - קבלת בקשות מ-Flapi לעיבוד אסינכרוני  
✅ **Smart Comparison** - השוואה חכמה של Body ו-Status Code בלבד  

## 🐳 Docker

להרצה ב-Docker:
\`\`\`bash
docker build -t diff-spectrum-backend .
docker run -p 5001:80 diff-spectrum-backend
\`\`\`

## 🔧 הגדרות נוספות

### הוספת Endpoints להשוואה
ערוך את `appsettings.json`:
\`\`\`json
"ApiComparison": {
  "ProductionBaseUrl": "https://api.production.com",
  "IntegrationBaseUrl": "https://api.integration.com",
  "Endpoints": [
    "/api/users",
    "/api/products",
    "/api/orders"
  ]
}
\`\`\`

## 📝 הערות

- הקוד כתוב בצורה נקייה ומקצועית
- עוקב אחר עקרונות SOLID
- מוכן לסביבת Production
- תומך ב-Scalability
- משווה רק Body ו-Status Code (לא Headers)
- Job מוצלח לא מבטיח שאין Diffs
