# MyBlogApp - ASP.NET Core Blog Application

A full-featured blog application built with ASP.NET Core 9.0, Entity Framework Core, and SQL Server with complete authentication and authorization system.


## ✨ Features

### User Management
- ✅ User Registration with strong password validation
- ✅ User Login/Logout with secure cookie-based authentication
- ✅ Role-based authorization (Admin & User roles)
- ✅ Account lockout after 5 failed login attempts (5-minute lockout)
- ✅ "Remember Me" functionality for persistent login
- ✅ Auto-login after successful registration

### Blog Post Management
- ✅ Create, Read, Update, Delete (CRUD) operations for blog posts
- ✅ Rich text editor (Quill.js) for post content
- ✅ Feature image upload (JPG, JPEG, PNG)
- ✅ Post categorization (Technology, Health, Lifestyle)
- ✅ Category-based post filtering
- ✅ Post ownership - users can only edit/delete their own posts
- ✅ Admins can manage all posts
- ✅ Automatic author assignment from logged-in user

### Comments System
- ✅ Add comments to blog posts (requires login)
- ✅ Real-time comment addition using AJAX
- ✅ Display comment author and timestamp
- ✅ Automatic username assignment from logged-in user

### Security Features
- ✅ Password hashing using ASP.NET Core Identity
- ✅ Anti-forgery token validation on all POST requests
- ✅ Secure file upload with extension validation
- ✅ Role-based access control

## 🛠️ Technologies Used

### Backend
- **ASP.NET Core 9.0** - Web framework
- **Entity Framework Core 9.0** - ORM for database operations
- **ASP.NET Core Identity** - Authentication and authorization
- **SQL Server** - Database
- **C# 12** - Programming language

### Frontend
- **HTML5 & CSS3** - Markup and styling
- **Bootstrap 5.3** - CSS framework
- **JavaScript (ES6+)** - Client-side scripting
- **jQuery 3.6** - AJAX operations
- **Quill.js 2.0** - Rich text editor

### Tools & Libraries
- **Visual Studio 2022** / **VS Code** - IDE
- **SQL Server Management Studio (SSMS)** - Database management
- **Entity Framework Core Tools** - Migrations
- **jQuery Validation** - Client-side form validation
- **CDN** - Content delivery for libraries

## 🚀 Installation

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/MyBlogApp.git
cd MyBlogApp
```

### 2. Configure Database Connection

Create `appsettings.json` in the `MyBlogApp` folder (if not exists):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=MyBlogAppDb;User Id=yourid;Password=Yourdbpassword;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Replace `YOUR_SERVER_NAME` with your SQL Server instance name:**
- For LocalDB: `(localdb)\\MSSQLLocalDB`
- For SQL Server Express: `.\\SQLEXPRESS`
- For full SQL Server: `localhost` or your server name

### 3. Install Dependencies
```bash
cd MyBlogApp
dotnet restore
```

### 4. Apply Database Migrations
```bash
dotnet ef database update
```

This will:
- Create the database
- Create all tables (Posts, Categories, Comments, AspNetUsers, AspNetRoles, etc.)
- Seed initial data (categories and roles)
- Create default Admin user

### 5. Run the Application
```bash
dotnet run
```

The application will be available at:
- HTTPS: `https://localhost:7158`
- HTTP: `http://localhost:5196`

## 🗄️ Database Setup

The application uses Entity Framework Core Code-First approach. The database schema includes:

### Tables
- **Posts** - Blog posts with title, content, author, category, feature image
- **Categories** - Post categories (Technology, Health, Lifestyle)
- **Comments** - User comments on posts
- **AspNetUsers** - User accounts
- **AspNetRoles** - User roles (Admin, User)
- **AspNetUserRoles** - Links users to roles

### Seed Data
The application automatically seeds:
- 2 Roles: Admin, User
- 3 Categories: Technology, Health, Lifestyle
- 1 Admin user (see credentials below)

## 🔐 Default Admin Credentials

After running the application for the first time, you can login with:
```
Email: admin@myblog.com
Password: Admin@123
```

## 🔒 Authentication & Authorization

### Password Requirements
- Minimum 6 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (@#$%^&*)

### User Roles

#### Admin Role
- Can create, edit, and delete ANY post
- Can manage all content
- Has full access to all features

#### User Role
- Can create their own posts
- Can only edit/delete their own posts
- Can comment on any post
- Cannot manage other users' posts

### Access Control
- **Public Pages:** Post list (Index), Post details
- **Authenticated Pages:** Create post, Add comments
- **Owner/Admin Only:** Edit post, Delete post (only owner or admin can access)

## 📖 Usage

### For Visitors (Not Logged In)
1. Browse blog posts on the homepage
2. Filter posts by category
3. Read full post details
4. Register for an account to create posts and comment

### For Registered Users
1. **Register:** Click "Register" → Fill form → Auto-login
2. **Login:** Click "Login" → Enter email and password
3. **Create Post:** Click "Create Post" → Fill form with title, category, content, image → Submit
4. **Edit Your Post:** Click "Edit" on your post → Modify → Update
5. **Delete Your Post:** Click "Delete" on your post → Confirm deletion
6. **Add Comment:** Login → Go to post detail → Write comment → Submit

### For Admins
- All user features plus:
- Edit/delete ANY post (not just your own)
- Full content moderation capabilities







