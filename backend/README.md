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

## 🔄 Background Jobs

המערכת מריצה אוטומטית השוואת APIs כל שעה באמצעות Hangfire:
- משווה endpoints בין Production ל-Integration
- יוצר Diffs עבור הבדלים
- מעדכן סטטיסטיקות Jobs

## 🎯 תכונות

✅ **Repository Pattern** - הפרדה בין Business Logic ל-Data Access  
✅ **Dependency Injection** - ניהול תלויות נקי  
✅ **Soft Delete** - מחיקה לוגית של נתונים  
✅ **Background Jobs** - עיבוד אסינכרוני עם Hangfire  
✅ **Swagger Documentation** - תיעוד API אוטומטי  
✅ **CORS Support** - תמיכה בקריאות מהקליינט  

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
