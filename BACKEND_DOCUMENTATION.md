# JobBoard Backend API Dokumentasyon

**Sürüm:** 1.0.0  
**Framework:** .NET 8  
**Database:** SQL Server  
**Authentication:** JWT (JSON Web Tokens)  
**API Format:** RESTful JSON  

---

## ?? ?çind?kil?r

1. [Sistem Arxitekturas?](#sistem-arxitekturas?)
2. [Database ?emas?](#database-?emas?)
3. [API Endpoints](#api-endpoints)
4. [Authentication & Authorization](#authentication--authorization)
5. [Error Handling](#error-handling)
6. [Data Transfer Objects (DTOs)](#data-transfer-objects-dtos)
7. [Kurulum & Konfigürasyon](#kurulum--konfigürasyon)
8. [Frontend ?nteqrasyon](#frontend-inteqrasyon)

---

## ??? Sistem Arxitekturas?

Backend 4 ana layer? bölünmü?dür:

```
JobBoard.API (Presentation Layer)
    ?
JobBoard.Core (Domain Layer - Entities, Interfaces, DTOs)
    ?
JobBoard.Infrastructure (Data Access Layer - Services, Database)
    ?
JobBoard.Application (Business Logic Layer - Validators)
```

### Layerl?rin M?suliyy?tl?ri

#### 1?? **JobBoard.API** (Presentation)
- HTTP endpoints (Controllers)
- Request/Response i?l?m?si
- Middleware (Exception handling, Authentication)
- Rate limiting
- CORS policy

#### 2?? **JobBoard.Core** (Domain)
- **Entities:** Database model'l?ri
- **Interfaces:** Service kontraktlar?
- **DTOs:** Data transfer object'l?ri
- **Settings:** Configuration class'lar?
- **Exceptions:** Custom exception'lar

#### 3?? **JobBoard.Infrastructure** (Data Access)
- **Services:** Business logic implementasiyas?
- **Data:** Database context ve migrations
- **Migrations:** EF Core migrations

#### 4?? **JobBoard.Application** (Business Logic)
- **Validators:** FluentValidation validators

---

## ?? Database ?emas?

### ?sas Entityl?r

#### **1. User (Istifad?çi)**
```csharp
// Üç rol mövcuddur: candidate, employer, admin
Id              ? Primary Key
FullName        ? Ad-Soyad
Email           ? Unikal email (soft-delete filter il?)
PasswordHash    ? BCrypt il? hashl?nmi? ?ifr?
Role            ? candidate | employer | admin
AvatarUrl       ? Profil ??kili
Phone           ? Telefon nömr?si
IsEmailVerified ? Email do?rulanm??m??
IsActive        ? Aktiv hesab?
IsDeleted       ? Soft delete flag
RefreshToken    ? JWT refresh token'?
CreatedAt       ? Yarad?l?? tarixi
UpdatedAt       ? Yenil?nm? tarixi
```

**Related Entities:**
- `CandidateProfile` ? Namiz?d profili
- `Company` ? ?irk?t profili
- `SavedJob` ? Yadda saxlanm?? i? elanlar?
- `JobApplication` ? ?? müraci?tl?ri
- `JobAlert` ? ?? bildiri?l?ri
- `Transaction` ? Öd?ni? ?m?liyyatlar?

---

#### **2. Job (?? Elan?)**
```csharp
Id                  ? Primary Key
CompanyId           ? ?irk?tin ID'si
CategoryId          ? Kateqoriyan?n ID'si
Title               ? ?? ba?l???
Slug                ? URL-friendly versiya (unikal)
Description         ? T?svir
Requirements        ? T?l?bl?r
Responsibilities    ? M?suliyy?tl?r
Benefits            ? Üstünlükl?r
Location            ? Yerl??m?
IsRemote            ? Uzaqdan i??
JobType             ? Full-time | Part-time | Contract | Internship
ExperienceLevel     ? Junior | Mid | Senior
SalaryMin           ? Minimum maa?
SalaryMax           ? Maksimum maa?
SalaryCurrency      ? Valyuta (USD, EUR, AZN...)
IsSalaryVisible     ? Maa? göst?rilsin mi?
Status              ? active | closed | draft
IsFeatured          ? Ön plana ç?xar?lm???
IsUrgent            ? Fövq?lad??
IsDeleted           ? Soft delete flag
Deadline            ? Son müraci?t tarixi
ViewCount           ? Bax?? say?
CreatedAt           ? Yarad?l?? tarixi
UpdatedAt           ? Yenil?nm? tarixi
```

**Related Entities:**
- `Company` ? ?lan? yerl??dir?n ?irk?t
- `Category` ? ?? kateqoriyas?
- `JobSkill` ? T?l?b olunan bacar?qlar
- `JobApplication` ? Müraci?tl?r
- `SavedJob` ? Yadda saxlamalar

---

#### **3. Company (?irk?t)**
```csharp
Id                  ? Primary Key
UserId              ? ?irk?t sahibinin User ID'si
Name                ? ?irk?t ad?
LogoUrl             ? Logo ??kili
CoverImageUrl       ? Fon ??kili
Description         ? ?irk?t haqq?nda
Industry            ? S?naye sah?si
CompanySize         ? ?irk?t ölçüsü (1-10, 11-50...)
Website             ? Veb sayt
Location            ? ?sas ofis yeri
Phone               ? Telefon
Email               ? Email
FoundedYear         ? T?sisil tarixi
IsVerified          ? T?sdiq olunmu??
SocialFacebook      ? Facebook link
SocialTwitter       ? Twitter link
SocialLinkedIn      ? LinkedIn link
CreatedAt           ? Yarad?l?? tarixi
UpdatedAt           ? Yenil?nm? tarixi
```

**Related Entities:**
- `User` ? ?irk?t sahibi
- `Job` ? ?irk?tin i? elanlar?
- `CompanyReview` ? ?irk?t r?yl?ri

---

#### **4. CandidateProfile (Namiz?d Profili)**
```csharp
Id                  ? Primary Key
UserId              ? Namiz?din User ID'si
Headline            ? Ba?l?q (m?s: "Full Stack Developer")
Summary             ? Xülas?/Bio
Location            ? Yerl??m?
Website             ? ??xsi veb sayt
LinkedInUrl         ? LinkedIn profili
GithubUrl           ? GitHub profili
ExperienceYears     ? T?crüb? ill?ri
CurrentPosition     ? Cari mövqe
ExpectedSalary      ? Gözl?nil?n maa?
IsAvailable         ? ?? axtar???nda m??
ResumeUrl           ? CV/Resume fayl?
VideoResumeUrl      ? Video CV (opsional)
```

**Related Entities:**
- `User` ? Namiz?d
- `CandidateSkill` ? Bacar?qlar
- `WorkExperience` ? ?? t?crüb?si
- `Education` ? T?hsil
- `Portfolio` ? Portfolio proyektl?ri
- `CandidateLanguage` ? Dil biliyi

---

#### **5. JobApplication (?? Müraci?ti)**
```csharp
Id                  ? Primary Key
JobId               ? ?? elan?n?n ID'si
CandidateId         ? Namiz?din User ID'si
ApplicantName       ? Müraci?tçinin ad?
ApplicantEmail      ? Müraci?tçinin emaili
CoverLetter         ? Müraci?t m?ktubu
Resume              ? CV/Resume
Status              ? applied | reviewing | shortlisted | rejected | accepted | withdrawn
Rating              ? D?y?rlendirm? (1-5)
Notes               ? Qeydl?r
CreatedAt           ? Müraci?t tarixi
UpdatedAt           ? Yenil?nm? tarixi
```

---

#### **6. BlogPost (Bloqun M?qal?si)**
```csharp
Id                  ? Primary Key
AuthorId            ? Mü?llifin User ID'si (admin)
Title               ? Ba?l?q
Slug                ? URL-friendly versiya
Content             ? M?qal? m?tni (HTML)
Summary             ? Q?sa xülas?
FeaturedImageUrl    ? Ön ??kil
Category            ? Kateqoriya
Tags                ? Teql?r
ViewCount           ? Bax?? say?
IsPublished         ? D?rc olunmu??
IsDeleted           ? Soft delete flag
PublishedAt         ? D?rc tarixi
CreatedAt           ? Yarad?l?? tarixi
UpdatedAt           ? Yenil?nm? tarixi
```

---

#### **7. Transaction (Öd?ni? ?m?liyyat?)**
```csharp
Id                  ? Primary Key
UserId              ? ?stifad?çinin User ID'si
PlanId              ? Seçil?n plan
Amount              ? M?bl??
Currency            ? Valyuta
Status              ? pending | processing | completed | failed | refunded
TransactionType     ? job_posting | featured_job | premium_subscription
PaymentMethod       ? card | bank_transfer
OrderId             ? Sifari? ID'si (payment gateway'd?n)
CardLast4           ? Kart?n son 4 r?q?mi
ExpiryMonth         ? Kart?n son i?l?nm? ay?
ExpiryYear          ? Kart?n son i?l?nm? ili
CreatedAt           ? Yarad?l?? tarixi
UpdatedAt           ? Yenil?nm? tarixi
```

---

### Relational Diagram
```
User (1) ??? (1) CandidateProfile
       ?       ?? WorkExperience
       ?       ?? Education
       ?       ?? CandidateSkill
       ?       ?? Portfolio
       ?
       ???? (1) Company ??? (M) Job ??? (M) JobApplication
       ?                          ?           ?? Candidate
       ?                          ?? JobSkill
       ?                          ?? SavedJob
       ?
       ???? (M) JobApplication
       ?
       ???? (M) SavedJob ????????????? Job
       ?
       ???? (M) JobAlert
       ?
       ???? (M) Transaction
       ?
       ???? (M) BlogComment

BlogPost (1) ??? (M) BlogComment ??? User
```

---

## ?? API Endpoints

### Base URL
```
https://localhost:5001/api
```

---

### ?? **AUTH Endpoints** (`/api/auth`)

#### 1. Qeydiyyat (Register)
```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Hüseyn Hüseynov",
  "email": "huseyn@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "role": "candidate"  // veya "employer"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Do?rulama emaili gönd?rildi. Z?hm?t olmasa emailinizi yoxlay?n."
}
```

**Mümkün Hatalar:**
- `409` - Email art?q istifad? olunur
- `400` - Daxil edil?n m?lumatlar yanl??

---

#### 2. Giri? (Login)
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "huseyn@example.com",
  "password": "SecurePass123!",
  "rememberMe": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "expiresIn": 900,  // saniy? (15 d?qiq?)
    "user": {
      "id": 1,
      "fullName": "Hüseyn Hüseynov",
      "email": "huseyn@example.com",
      "role": "candidate",
      "avatarUrl": null
    }
  }
}
```

**Mümkün Hatalar:**
- `401` - Email/?ifr? yanl??
- `401` - Email do?rulanmam??

---

#### 3. Token Yenil? (Refresh)
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a12bc34d-efgh-5678-ijkl-9mnopqrs1234",
    "expiresIn": 900
  }
}
```

---

#### 4. Email Do?rulama (Verify)
```http
GET /api/auth/verify-email?token=abc123def456
```

**Response:** Redirect ediir `http://localhost:3000/login?verified=true`

---

#### 5. ?ifr? S?f?rlama ?steyi (Forgot Password)
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "huseyn@example.com"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "?g?r bu email mövcuddursa, s?f?rlama linki gönd?rildi."
}
```

---

#### 6. ?ifr? S?f?rlama (Reset Password)
```http
POST /api/auth/reset-password
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "token": "reset-token",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

---

#### 7. Ç?x?? (Logout)
```http
POST /api/auth/logout
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "refreshToken": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "U?urla ç?x?? edildi."
}
```

---

### ?? **JOBS Endpoints** (`/api/jobs`)

#### 1. ?? Elanlar? Siyah?s? (Get All Jobs)
```http
GET /api/jobs?keyword=developer&location=Baku&categoryId=1&jobType=Full-time&salary_min=1000&salary_max=5000&isRemote=true&page=1&pageSize=10&sortBy=newest
```

**Query Parameters:**
| Parametr | Tip | T?svir |
|----------|-----|--------|
| `keyword` | string | Axtar?? sözü |
| `location` | string | Yerl??m? |
| `categoryId` | int | Kateqoriya ID'si |
| `jobType` | string | ?? tipi (Full-time, Part-time...) |
| `experienceLevel` | string | T?crüb? s?viyy?si |
| `isRemote` | bool | Uzaqdan i?? |
| `isFeatured` | bool | Ön plana ç?xar?lm??? |
| `isUrgent` | bool | Fövq?lad?? |
| `companyId` | int | ?irk?t ID'si |
| `salary_min` | decimal | Minimum maa? |
| `salary_max` | decimal | Maksimum maa? |
| `sortBy` | string | newest, oldest, salary_asc, salary_desc |
| `page` | int | S?hif? nömr?si (default: 1) |
| `pageSize` | int | S?hif? ölçüsü (max: 50) |

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Senior Full Stack Developer",
        "slug": "senior-full-stack-developer",
        "company": {
          "id": 1,
          "name": "TechCorp",
          "logoUrl": "https://..."
        },
        "category": {
          "id": 1,
          "name": "Software Development"
        },
        "location": "Bak?",
        "isRemote": true,
        "jobType": "Full-time",
        "experienceLevel": "Senior",
        "salaryMin": 3000,
        "salaryMax": 5000,
        "salaryCurrency": "USD",
        "isFeatured": true,
        "isUrgent": false,
        "viewCount": 150,
        "deadline": "2025-12-31T23:59:59Z",
        "createdAt": "2025-06-15T10:30:00Z",
        "isSaved": false  // Cari istifad?çi üçün
      }
      // ... daha çox i? elan?
    ],
    "totalCount": 45,
    "pageCount": 5,
    "hasNextPage": true
  }
}
```

---

#### 2. ?? Elan? Detaylar? (Get Job By ID)
```http
GET /api/jobs/1
Authorization: Bearer {accessToken}  // Opsional
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Senior Full Stack Developer",
    "slug": "senior-full-stack-developer",
    "description": "Detailed job description...",
    "requirements": "Requirements list...",
    "responsibilities": "Responsibilities...",
    "benefits": "Benefits...",
    "company": {
      "id": 1,
      "name": "TechCorp",
      "logoUrl": "https://...",
      "description": "Company description...",
      "website": "https://techcorp.az",
      "location": "Bak?",
      "industry": "Technology",
      "isVerified": true
    },
    "category": {
      "id": 1,
      "name": "Software Development"
    },
    "location": "Bak?",
    "isRemote": true,
    "jobType": "Full-time",
    "experienceLevel": "Senior",
    "salaryMin": 3000,
    "salaryMax": 5000,
    "salaryCurrency": "USD",
    "isSalaryVisible": true,
    "status": "active",
    "isFeatured": true,
    "isUrgent": false,
    "viewCount": 150,
    "deadline": "2025-12-31T23:59:59Z",
    "requiredSkills": [
      {
        "id": 1,
        "skillName": "React"
      },
      {
        "id": 2,
        "skillName": "Node.js"
      }
    ],
    "applicationCount": 25,
    "isSaved": false,
    "hasApplied": false,
    "createdAt": "2025-06-15T10:30:00Z",
    "updatedAt": "2025-06-15T10:30:00Z"
  }
}
```

---

#### 3. ?? Elan?n? Slug-a Gör? ?ld? Et (Get By Slug)
```http
GET /api/jobs/slug/senior-full-stack-developer
```

Response eyni il? `GET /api/jobs/{id}`

---

#### 4. Ön Plana Ç?xar?lm?? ?? Elanlar? (Featured Jobs)
```http
GET /api/jobs/featured
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    // Max 10 ön plana ç?xar?lm?? i? elan?
  ]
}
```

---

#### 5. ?? Elan? Yaratma (Create Job) ? Admin/Employer
```http
POST /api/jobs
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "Junior Frontend Developer",
  "description": "Detailed job description...",
  "requirements": "Required skills...",
  "responsibilities": "Responsibilities...",
  "benefits": "Benefits...",
  "location": "Bak?",
  "isRemote": false,
  "jobType": "Full-time",
  "experienceLevel": "Junior",
  "categoryId": 1,
  "salaryMin": 1500,
  "salaryMax": 2500,
  "salaryCurrency": "USD",
  "salaryPeriod": "month",
  "isSalaryVisible": true,
  "isUrgent": false,
  "deadline": "2025-12-31T23:59:59Z",
  "requiredSkills": ["React", "JavaScript", "CSS"]
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "?? elan? yarad?ld?.",
  "data": {
    "id": 5,
    "title": "Junior Frontend Developer",
    // ... tam i? elan? detaylar?
  }
}
```

---

#### 6. ?? Elan?n? Yenil? (Update Job) ? Admin/Employer
```http
PUT /api/jobs/1
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  // Eyni request body il? Create endpoint'ind?
}
```

---

#### 7. ?? Elan?n? Sil (Delete Job) ? Admin/Employer
```http
DELETE /api/jobs/1
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "?? elan? silindi."
}
```

---

#### 8. ?? Elan? Statusunu D?yi? (Update Status) ? Admin/Employer
```http
PATCH /api/jobs/1/status
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "status": "closed"  // active, closed, draft
}
```

---

#### 9. ?? Elan?n? Ön Plana Ç?xart (Featured) ? Admin
```http
PATCH /api/jobs/1/featured
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "isFeatured": true
}
```

---

#### 10. M?nim ?? Elanlar?m (My Jobs) ? Employer
```http
GET /api/jobs/my?status=active&page=1&pageSize=10
Authorization: Bearer {accessToken}
```

---

### ?? **APPLICATIONS Endpoints** (`/api/applications`)

#### 1. Müraci?t Yaratma (Apply for Job) ? Candidate
```http
POST /api/applications
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "jobId": 1,
  "coverLetter": "I am interested in this position because..."
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Müraci?tiniz gönd?rildi.",
  "data": {
    "id": 1,
    "jobId": 1,
    "jobTitle": "Senior Full Stack Developer",
    "candidateId": 5,
    "candidateName": "Hüseyn Hüseynov",
    "status": "applied",
    "createdAt": "2025-06-15T10:30:00Z"
  }
}
```

---

#### 2. M?nim Müraci?tl?rim (My Applications) ? Candidate
```http
GET /api/applications/my?status=applied&page=1&pageSize=10
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "jobId": 1,
        "jobTitle": "Senior Full Stack Developer",
        "company": {
          "id": 1,
          "name": "TechCorp",
          "logoUrl": "https://..."
        },
        "status": "applied",
        "createdAt": "2025-06-15T10:30:00Z"
      }
    ],
    "totalCount": 3,
    "pageCount": 1,
    "hasNextPage": false
  }
}
```

---

#### 3. Müraci?t Detaylar? (Get Application)
```http
GET /api/applications/1
Authorization: Bearer {accessToken}
```

---

#### 4. Müraci?t Statusunu Yenil? (Update Status) ? Employer
```http
PATCH /api/applications/1/status
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "status": "shortlisted",  // applied, reviewing, shortlisted, rejected, accepted
  "notes": "Qualified candidate, schedule interview"
}
```

---

#### 5. Müraci?ti Geri Ç?k (Withdraw) ? Candidate
```http
PATCH /api/applications/1/withdraw
Authorization: Bearer {accessToken}
```

---

#### 6. Müraci?t Statistikas? (Stats) ? Employer
```http
GET /api/applications/stats
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalApplications": 150,
    "appliedCount": 50,
    "reviewingCount": 30,
    "shortlistedCount": 20,
    "acceptedCount": 10,
    "rejectedCount": 40,
    "withdrawnCount": 0
  }
}
```

---

### ?? **CANDIDATES Endpoints** (`/api/candidates`)

#### 1. Namiz?dl?r Siyah?s? (Get Candidates) ? Employer
```http
GET /api/candidates?keyword=developer&experience=5&page=1&pageSize=10
Authorization: Bearer {accessToken}
Roles: employer, admin
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "userId": 1,
        "fullName": "Hüseyn Hüseynov",
        "email": "huseyn@example.com",
        "headline": "Full Stack Developer",
        "location": "Bak?",
        "experienceYears": 5,
        "avatarUrl": "https://...",
        "currentPosition": "Senior Developer at TechCorp"
      }
    ],
    "totalCount": 45,
    "pageCount": 5,
    "hasNextPage": true
  }
}
```

---

#### 2. Namiz?d Profili (Get Candidate Profile)
```http
GET /api/candidates/1
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "fullName": "Hüseyn Hüseynov",
    "email": "huseyn@example.com",
    "avatarUrl": "https://...",
    "headline": "Full Stack Developer",
    "summary": "Experienced developer with 5 years...",
    "location": "Bak?",
    "website": "https://huseyn.dev",
    "linkedInUrl": "https://linkedin.com/in/huseyn",
    "githubUrl": "https://github.com/huseyn",
    "experienceYears": 5,
    "currentPosition": "Senior Developer",
    "expectedSalary": "3000-5000",
    "isAvailable": true,
    "resumeUrl": "https://...",
    "videoResumeUrl": null,
    "skills": [
      {
        "id": 1,
        "name": "React",
        "endorsements": 15
      }
    ],
    "workExperiences": [
      {
        "id": 1,
        "company": "TechCorp",
        "position": "Senior Developer",
        "startDate": "2020-01-01T00:00:00Z",
        "endDate": null,
        "isCurrent": true,
        "description": "Lead frontend development..."
      }
    ],
    "educations": [
      {
        "id": 1,
        "institution": "Baku State University",
        "degree": "Bachelor",
        "fieldOfStudy": "Computer Science",
        "startDate": "2015-09-01T00:00:00Z",
        "endDate": "2019-06-01T00:00:00Z"
      }
    ]
  }
}
```

---

#### 3. M?nim Profilim (Get My Profile) ? Candidate
```http
GET /api/candidates/me
Authorization: Bearer {accessToken}
```

---

#### 4. Profili Yenil? (Update Profile) ? Candidate
```http
PUT /api/candidates/me
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "headline": "Senior Full Stack Developer",
  "summary": "Experienced developer...",
  "location": "Bak?",
  "website": "https://mysite.com",
  "linkedInUrl": "https://linkedin.com/in/me",
  "githubUrl": "https://github.com/me",
  "experienceYears": 5,
  "currentPosition": "Senior Developer",
  "expectedSalary": "4000-6000",
  "isAvailable": true
}
```

---

#### 5. ?? T?crüb?si ?lav? Et (Add Experience) ? Candidate
```http
POST /api/candidates/me/experience
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "company": "TechCorp",
  "position": "Senior Developer",
  "startDate": "2020-01-01T00:00:00Z",
  "endDate": null,
  "isCurrent": true,
  "description": "Lead frontend development..."
}
```

---

#### 6. ?? T?crüb?sini Yenil? (Update Experience) ? Candidate
```http
PUT /api/candidates/me/experience/1
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  // Eyni request body il? ?lav? endpoint'ind?
}
```

---

#### 7. ?? T?crüb?sini Sil (Delete Experience) ? Candidate
```http
DELETE /api/candidates/me/experience/1
Authorization: Bearer {accessToken}
```

---

#### 8. T?hsil ?lav? Et (Add Education) ? Candidate
```http
POST /api/candidates/me/education
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "institution": "Baku State University",
  "degree": "Bachelor",
  "fieldOfStudy": "Computer Science",
  "startDate": "2015-09-01T00:00:00Z",
  "endDate": "2019-06-01T00:00:00Z"
}
```

---

#### 9. Bacar?q ?lav? Et (Add Skill) ? Candidate
```http
POST /api/candidates/me/skills
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "skillName": "React"
}
```

---

#### 10. Portfolio ?lav? Et (Add Portfolio) ? Candidate
```http
POST /api/candidates/me/portfolio
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "E-Commerce Platform",
  "description": "Full-stack project...",
  "projectUrl": "https://github.com/project",
  "imageUrl": "https://...",
  "technologies": ["React", "Node.js", "MongoDB"]
}
```

---

### ?? **COMPANIES Endpoints** (`/api/companies`)

#### 1. ?irk?tl?r Siyah?s? (Get Companies)
```http
GET /api/companies?keyword=tech&industry=Technology&location=Baku&page=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "TechCorp",
        "logoUrl": "https://...",
        "location": "Bak?",
        "industry": "Technology",
        "companySize": "50-100",
        "website": "https://techcorp.az",
        "jobsCount": 15,
        "reviewsCount": 5,
        "avgRating": 4.5,
        "isVerified": true
      }
    ],
    "totalCount": 25,
    "pageCount": 3,
    "hasNextPage": true
  }
}
```

---

#### 2. ?irk?t Profili (Get Company)
```http
GET /api/companies/1
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "TechCorp",
    "logoUrl": "https://...",
    "coverImageUrl": "https://...",
    "description": "Leading technology company...",
    "industry": "Technology",
    "companySize": "50-100",
    "website": "https://techcorp.az",
    "location": "Bak?",
    "phone": "+994 50 XXX XX XX",
    "email": "hr@techcorp.az",
    "foundedYear": "2010",
    "isVerified": true,
    "socialFacebook": "https://facebook.com/techcorp",
    "socialTwitter": "https://twitter.com/techcorp",
    "socialLinkedIn": "https://linkedin.com/company/techcorp",
    "activeJobsCount": 15,
    "totalJobsCount": 150,
    "reviews": [
      {
        "id": 1,
        "rating": 5,
        "title": "Great company to work for",
        "comment": "Amazing culture and benefits",
        "authorName": "John Doe",
        "createdAt": "2025-06-15T10:30:00Z"
      }
    ],
    "avgRating": 4.5
  }
}
```

---

#### 3. M?nim ?irk?tim (My Company) ? Employer
```http
GET /api/companies/me
Authorization: Bearer {accessToken}
```

---

#### 4. ?irk?t Profili Yenil? (Update Company) ? Employer
```http
PUT /api/companies/me
Authorization: Bearer {accessToken}
Content-Type: multipart/form-data

{
  "name": "TechCorp Inc.",
  "description": "Updated description...",
  "industry": "Technology",
  "companySize": "100-500",
  "website": "https://newtechcorp.az",
  "location": "Bak?",
  "phone": "+994 50 XXX XX XX",
  "email": "hr@newtechcorp.az",
  "foundedYear": "2010",
  "socialFacebook": "https://facebook.com/techcorp",
  "socialTwitter": "https://twitter.com/techcorp",
  "socialLinkedIn": "https://linkedin.com/company/techcorp",
  "logo": <file>,
  "coverImage": <file>
}
```

---

#### 5. ?irk?t R?yi Yaratma (Add Review) ? Candidate
```http
POST /api/companies/1/reviews
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "rating": 5,
  "title": "Great company to work for",
  "comment": "Amazing culture and benefits"
}
```

---

### ?? **BLOG Endpoints** (`/api/blog`)

#### 1. M?qal?l?r Siyah?s? (Get Posts)
```http
GET /api/blog/posts?keyword=javascript&category=tutorial&page=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Getting Started with React Hooks",
        "slug": "getting-started-with-react-hooks",
        "summary": "A beginner's guide to React Hooks...",
        "author": {
          "id": 1,
          "fullName": "Admin User"
        },
        "featuredImageUrl": "https://...",
        "category": "Tutorial",
        "tags": ["React", "JavaScript"],
        "viewCount": 500,
        "publishedAt": "2025-06-15T10:30:00Z"
      }
    ],
    "totalCount": 25,
    "pageCount": 3,
    "hasNextPage": true
  }
}
```

---

#### 2. M?qal? Detaylar? (Get Post By Slug)
```http
GET /api/blog/posts/getting-started-with-react-hooks
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Getting Started with React Hooks",
    "slug": "getting-started-with-react-hooks",
    "content": "<h1>Getting Started...</h1><p>Lorem ipsum...</p>",
    "summary": "A beginner's guide to React Hooks...",
    "author": {
      "id": 1,
      "fullName": "Admin User",
      "avatarUrl": "https://..."
    },
    "featuredImageUrl": "https://...",
    "category": "Tutorial",
    "tags": ["React", "JavaScript"],
    "viewCount": 500,
    "comments": [
      {
        "id": 1,
        "content": "Great article!",
        "author": {
          "id": 5,
          "fullName": "John Doe"
        },
        "createdAt": "2025-06-15T10:30:00Z"
      }
    ],
    "publishedAt": "2025-06-15T10:30:00Z",
    "createdAt": "2025-06-15T10:30:00Z"
  }
}
```

---

#### 3. M?qal? Yaratma (Create Post) ? Admin
```http
POST /api/blog/posts
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "Getting Started with React Hooks",
  "content": "<h1>Getting Started...</h1><p>Lorem ipsum...</p>",
  "summary": "A beginner's guide to React Hooks...",
  "category": "Tutorial",
  "tags": ["React", "JavaScript"],
  "featuredImageUrl": "https://..."
}
```

---

#### 4. M?qal? Yenil? (Update Post) ? Admin
```http
PUT /api/blog/posts/1
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  // Eyni request body il? ?lav? endpoint'ind?
}
```

---

#### 5. M?qal? Sil (Delete Post) ? Admin
```http
DELETE /api/blog/posts/1
Authorization: Bearer {accessToken}
```

---

#### 6. ??rh ?lav? Et (Add Comment) ? Authenticated
```http
POST /api/blog/posts/1/comments
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "content": "Great article, thanks for sharing!"
}
```

---

#### 7. ??rh Sil (Delete Comment) ? Authenticated
```http
DELETE /api/blog/comments/1
Authorization: Bearer {accessToken}
```

---

### ?? **TRANSACTIONS Endpoints** (`/api/transactions`)

#### 1. ?m?liyyatlar Siyah?s? (Get Transactions) ? Authenticated
```http
GET /api/transactions?status=completed&type=job_posting&page=1&pageSize=10
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "amount": 49.99,
        "currency": "USD",
        "status": "completed",
        "transactionType": "featured_job",
        "paymentMethod": "card",
        "cardLast4": "4242",
        "createdAt": "2025-06-15T10:30:00Z"
      }
    ],
    "totalCount": 10,
    "pageCount": 1,
    "hasNextPage": false
  }
}
```

---

#### 2. ?m?liyyat Detaylar? (Get Transaction)
```http
GET /api/transactions/1
Authorization: Bearer {accessToken}
```

---

#### 3. Öd?ni? Edin (Create Transaction) ? Employer
```http
POST /api/transactions
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "planId": 1,
  "cardNumber": "4242424242424242",
  "cardExpiry": "12/25",
  "cardCvv": "123",
  "cardName": "John Doe"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Öd?ni? u?urla tamamland?.",
  "data": {
    "id": 1,
    "amount": 49.99,
    "currency": "USD",
    "status": "completed",
    "orderId": "ORD-123456"
  }
}
```

---

#### 4. Faktura ?ld? Et (Get Invoice) ? Authenticated
```http
GET /api/transactions/1/invoice
Authorization: Bearer {accessToken}
```

**Response:** PDF/TXT fayl? download edilir

---

#### 5. Planlar? ?ld? Et (Get Plans)
```http
GET /api/transactions/plans
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Single Job Post",
      "description": "Post 1 job",
      "amount": 24.99,
      "currency": "USD",
      "duration": "30 days",
      "features": ["Post 1 job", "7-day featured option"]
    },
    {
      "id": 2,
      "name": "5 Job Posts",
      "description": "Post 5 jobs",
      "amount": 99.99,
      "currency": "USD",
      "duration": "90 days",
      "features": ["Post 5 jobs", "14-day featured option", "Priority support"]
    }
  ]
}
```

---

### ?? **ADMIN Endpoints** (`/api/admin`)

#### 1. Admin Dashboard (Dashboard Stats) ? Admin
```http
GET /api/admin/dashboard
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalUsers": 150,
    "totalJobs": 250,
    "totalApplications": 500,
    "totalCompanies": 30,
    "newUsersThisMonth": 25,
    "newJobsThisMonth": 45,
    "revenueThisMonth": 5000.00,
    "jobsByStatus": {
      "active": 200,
      "closed": 40,
      "draft": 10
    },
    "topCategories": [
      {
        "name": "Software Development",
        "jobCount": 100
      }
    ]
  }
}
```

---

#### 2. ?irk?tl?ri Yönetm? (Manage Companies) ? Admin
```http
GET /api/admin/companies?page=1&pageSize=10
Authorization: Bearer {accessToken}
```

---

#### 3. ?stifad?çil?ri Yönetm? (Manage Users) ? Admin
```http
GET /api/admin/users?role=employer&isActive=true&page=1&pageSize=10
Authorization: Bearer {accessToken}
```

---

#### 4. ?m?liyyatlar? Yönetm? (Manage Transactions) ? Admin
```http
GET /api/admin/transactions?status=completed&page=1&pageSize=10
Authorization: Bearer {accessToken}
```

---

#### 5. Kateqoriya Yaratma (Create Category) ? Admin
```http
POST /api/admin/categories
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Data Science",
  "description": "Data Science and Analytics jobs",
  "icon": "??"
}
```

---

## ?? Authentication & Authorization

### JWT Token Struktur
```
Header: { alg: "HS256", typ: "JWT" }

Payload: {
  sub: "1",              // User ID
  email: "user@test.com",
  role: "employer",      // candidate | employer | admin
  iat: 1623801600,
  exp: 1623802500       // 15 minutes later
}

Signature: HMACSHA256(header + "." + payload, SECRET)
```

### Token Müdd?ti
- **Access Token:** 15 d?qiq?
- **Refresh Token:** 7 gün (v? ya "Remember Me" varsa 30 gün)

### Authorization Header
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Rol ?sasl? D?st?k (Role-Based Access)
```
- candidate     ? Namiz?d (?? axtar???nda)
- employer      ? ???götür?n (?? elan? yerl??dir?r)
- admin         ? Administator (Bütün funksiyalara d?st?y)
```

### Protected Routes Nümun?l?ri
| Endpoint | Roller | ?zah |
|----------|--------|------|
| `POST /api/jobs` | employer, admin | Yaln?z i??götür?nl?r i? yerl??dir? bil?rl?r |
| `POST /api/applications` | candidate | Yaln?z namiz?dl?r müraci?t ed? bil?rl?r |
| `GET /api/admin/*` | admin | Yaln?z adminl?r admin panelini görm? bil?rl?r |
| `PUT /api/candidates/me` | candidate | Namiz?dl?r yaln?z öz profilini redakt? ed? bil?rl?r |

---

## ?? Error Handling

### Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Daxil edil?n m?lumatlar yanl??d?r.",
    "details": [
      {
        "field": "email",
        "message": "Email format? yanl??d?r"
      },
      {
        "field": "password",
        "message": "?ifr? ?n az? 8 simvol olmal?d?r"
      }
    ]
  }
}
```

### HTTP Status Kodlar?
| Kod | M?na | Nümun? |
|-----|------|--------|
| `200` | OK | Sorgulama u?urlu oldu |
| `201` | Created | Resurs yarad?ld? |
| `204` | No Content | Silm? u?urlu |
| `400` | Bad Request | Yanl?? daxil edil?n m?lumatlar |
| `401` | Unauthorized | Token'? yoxdur v? ya etibars?z |
| `403` | Forbidden | Bu ?m?liyyat? etm?y icaz?niz yoxdur |
| `404` | Not Found | Resurs tap?lmad? |
| `409` | Conflict | Email art?q istifad? olunur |
| `422` | Unprocessable Entity | D?y?rl?ndirm? x?tas? |
| `429` | Too Many Requests | Rate limit a??ld? |
| `500` | Server Error | Internal server error |

### Mümkün Error Kodlar?
```
VALIDATION_ERROR       - Daxil edil?n m?lumatlar yanl??
NOT_FOUND             - Resurs tap?lmad?
UNAUTHORIZED          - Giri? t?l?b olunur
FORBIDDEN             - D?st?yi yoxdur
CONFLICT              - Eyni resurs art?q mövcuddur
INVALID_TOKEN         - Token etibars?z
EXPIRED_TOKEN         - Token müdd?ti bitib
RATE_LIMIT_EXCEEDED   - Çox sayda sor?u
SERVER_ERROR          - Daxili server x?tas?
```

### Rate Limiting
- **Login endpoint:** 5 sor?u / d?qiq?
- **General API:** 100 sor?u / d?qiq?
- **File upload:** 10 MB maksimum

---

## ?? Data Transfer Objects (DTOs)

### Common DTOs

#### ApiResponse<T>
```csharp
{
  Success: bool,
  Data: T,
  Message: string,
  Error: ApiError
}
```

#### PagedResponse<T>
```csharp
{
  Items: List<T>,
  TotalCount: int,
  PageCount: int,
  HasNextPage: bool
}
```

#### ApiError
```csharp
{
  Code: string,
  Message: string,
  Details: List<FieldError>
}
```

### Auth DTOs

#### RegisterDto
```csharp
{
  FullName: string,
  Email: string,
  Password: string,
  ConfirmPassword: string,
  Role: string  // "candidate" | "employer"
}
```

#### LoginDto
```csharp
{
  Email: string,
  Password: string,
  RememberMe: bool
}
```

#### LoginResponseDto
```csharp
{
  AccessToken: string,
  RefreshToken: string,
  ExpiresIn: int,  // saniy?
  User: {
    Id: int,
    FullName: string,
    Email: string,
    Role: string,
    AvatarUrl: string
  }
}
```

---

## ?? Kurulum & Konfigürasyon

### Prerequisites
- .NET 8 SDK
- SQL Server (Express yet?rlidir)
- Visual Studio 2022 v? ya VS Code

### Ad?mlar

#### 1. Repository Klonla
```bash
git clone https://github.com/Sedmeq/JobBoard.git
cd JobBoard
```

#### 2. Database Ba?lant?s?n? Ayarla
`JobBoard.API/appsettings.json` düz?lt:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=JobBoardDb;Trusted_Connection=True;..."
  }
}
```

#### 3. NuGet Packages'? Qur
```bash
dotnet restore
```

#### 4. Database Migration'lar?n? T?tbiq Et
```bash
dotnet ef database update --project JobBoard.Infrastructure
```

#### 5. API'ni Ba?lat
```bash
cd JobBoard.API
dotnet run
```

API `https://localhost:5001` adresind? ba?lanacaq.

### appsettings.json Konfigürasyon

#### JWT Settings
```json
"JwtSettings": {
  "Secret": "your-super-secret-key-minimum-32-characters-long!",
  "Issuer": "JobBoardAPI",
  "Audience": "JobBoardClient",
  "AccessTokenExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}
```

#### Email Settings (Gmail)
```json
"Email": {
  "FromEmail": "your-email@gmail.com",
  "FromName": "JobBoard",
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUser": "your-email@gmail.com",
  "SmtpPass": "your-app-password"  // NOT regular password!
}
```

[Gmail App Password Qur](https://myaccount.google.com/apppasswords)

#### Storage Settings
```json
"Storage": {
  "Type": "Local",
  "LocalPath": "wwwroot/uploads",
  "BaseUrl": "https://localhost:5001"
}
```

#### CORS Settings
```json
"AllowedOrigins": "http://localhost:3000,http://localhost:5173"
```

---

## ?? Frontend ?nteqrasyon

### 1. Giri? Al??
```typescript
const login = async (email: string, password: string) => {
  const response = await fetch('https://localhost:5001/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  const data = await response.json();
  if (data.success) {
    localStorage.setItem('accessToken', data.data.accessToken);
    localStorage.setItem('refreshToken', data.data.refreshToken);
    return data.data.user;
  }
  throw new Error(data.error.message);
};
```

### 2. Protected Request
```typescript
const getJobs = async () => {
  const token = localStorage.getItem('accessToken');
  const response = await fetch('https://localhost:5001/api/jobs', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  const data = await response.json();
  if (!data.success) {
    // Handle error
    if (response.status === 401) {
      // Refresh token
      await refreshToken();
    }
  }
  return data.data;
};
```

### 3. Token Yenil?
```typescript
const refreshToken = async () => {
  const token = localStorage.getItem('refreshToken');
  const response = await fetch('https://localhost:5001/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: token })
  });

  const data = await response.json();
  if (data.success) {
    localStorage.setItem('accessToken', data.data.accessToken);
    localStorage.setItem('refreshToken', data.data.refreshToken);
  }
};
```

### 4. Axios Interceptor (React)
```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:5001/api'
});

// Request interceptor
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      await refreshToken();
      return api(error.config); // Retry request
    }
    return Promise.reject(error);
  }
);

export default api;
```

### 5. API Client Setup (React/Vue)
```typescript
// services/api.ts
export const JobBoardAPI = {
  // Auth
  auth: {
    register: (dto) => api.post('/auth/register', dto),
    login: (dto) => api.post('/auth/login', dto),
    logout: (dto) => api.post('/auth/logout', dto),
    refreshToken: (token) => api.post('/auth/refresh', { refreshToken: token })
  },

  // Jobs
  jobs: {
    getAll: (filter) => api.get('/jobs', { params: filter }),
    getById: (id) => api.get(`/jobs/${id}`),
    getBySlug: (slug) => api.get(`/jobs/slug/${slug}`),
    create: (dto) => api.post('/jobs', dto),
    update: (id, dto) => api.put(`/jobs/${id}`, dto),
    delete: (id) => api.delete(`/jobs/${id}`)
  },

  // Applications
  applications: {
    create: (dto) => api.post('/applications', dto),
    getMyApplications: (filter) => api.get('/applications/my', { params: filter }),
    getStats: () => api.get('/applications/stats'),
    updateStatus: (id, dto) => api.patch(`/applications/${id}/status`, dto)
  },

  // ... dig?r endpoints
};
```

### 6. Error Handling Pattern
```typescript
const handleApiError = (error: any) => {
  const response = error.response?.data;

  if (!response?.success) {
    const errorCode = response?.error?.code;
    const errorMsg = response?.error?.message;

    switch (errorCode) {
      case 'VALIDATION_ERROR':
        // Show field-level errors
        return response.error.details;
      case 'UNAUTHORIZED':
        // Redirect to login
        window.location.href = '/login';
        break;
      case 'NOT_FOUND':
        // Show not found message
        break;
      default:
        // Show generic error
        console.error(errorMsg);
    }
  }
};
```

### 7. File Upload
```typescript
const uploadResume = async (file: File) => {
  const formData = new FormData();
  formData.append('file', file);

  const response = await api.post('/candidates/me/resume', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });

  return response.data.data.resumeUrl;
};
```

---

## ?? ?lav? Qeydl?r

### Soft Delete
B?zi entityl?r (User, Job, Company, BlogPost) soft delete'? t?l? edilir. Bu o dem?kdir ki, `IsDeleted` flag'? `true` olduqda, h?min record'lar avtomatik olaraq sor?udan filtrl?nir.

### Slugification
?? elanlar? v? bloqun m?qal?l?ri unikal slug'lara malikdir. Slug'lar otomatik olaraq ba?l?qdan yarad?l?r:
- Böyük h?rfl?r kiçik h?rfl?r? çevrilir
- Bo?luqlar tire? çevrilir
- Xüsusi simvollar silinir

**Nümun?:** "Senior Full Stack Developer" ? "senior-full-stack-developer"

### View Count
?? elanlar? göründükd? (GET endpoint'i ziyar?t olduqda), bax?? say? avtomatik olaraq art?r?l?r.

### Pagination
Siyah? endpoint'l?ri `PagedResponse<T>` qaytar?r. Varsay?lan s?hif? ölçüsü 10, maksimum 50.

### Image Upload
Profil ??kill?ri, logo'lar v? dig?r ??kill?r lokal `wwwroot/uploads` qovlu?una saxlan?l?r.

### Email Templates
Email'l?ri HTML template'l?ri il? gönd?rilir. ?ablonlar:
- Email verification
- Password reset
- Application notification
- Payment receipt

---

## ?? Tez-tez Soru?ulan Suallar

### Frontend'in API'y nec? qura?d?raca??n?
Bax: [Frontend ?nteqrasyon](#frontend-inteqrasyon) bölümü

### Yeni m?qamlar? nec? ?lav? ed?c?yi
1. `JobBoard.Core/Entities/` qovlu?unda yeni Entity yarad?n
2. `AppDbContext` üz?rind? DbSet ?lav? edin
3. Migration yarad?n: `dotnet ef migrations add "DescriptiveName"`
4. Migration'? t?tbiq edin: `dotnet ef database update`
5. DTO'lar yarad?n v? Service logic'i ?lav? edin

### Xüll?ri nec? custom ed? bil?c?yi
`JobBoard.API/Middleware/ExceptionMiddleware.cs` fayl?n? redakt? edin. Orada bütün exception'lar?n i?l?nm?si ba? verir.

### Rate limiting'i nec? d?yi??c?yi
`JobBoard.API/Extensions/ServiceExtensions.cs` fayl?nda `AddRateLimiting` metoduna bax?n.

---

## ?? ?laq? & D?st?k

- **GitHub:** https://github.com/Sedmeq/JobBoard
- **Issues:** H?r hans? problem üçün GitHub issues aç?n

---

**Ax?r?nc? Yenil?m?:** 2025-06-15  
**Versiya:** 1.0.0
