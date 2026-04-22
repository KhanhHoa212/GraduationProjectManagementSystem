# 🎓 Graduation Project Management System (GPMS)

## 📌 Overview

**Graduation Project Management System (GPMS)** is a centralized web-based platform designed to manage the entire lifecycle of graduation (capstone) projects at universities.

Currently, project management processes are fragmented across multiple tools such as:
- Email  
- Excel  
- Google Drive  
- Chat platforms (Zalo, Microsoft Teams)  

This fragmentation leads to:
- Lack of transparency  
- Difficulty tracking project progress  
- Inconsistent evaluation processes  
- High administrative workload  

GPMS addresses these issues by providing a **unified system** to streamline and standardize project management workflows.

> 📄 Based on real-world problem analysis at FPT University

---

## 🎯 Objectives

### 🎯 General Objective

Build a system that manages the full lifecycle of graduation projects:
- Topic proposal  
- Approval  
- Progress tracking  
- Review & evaluation  
- Final defense  

### 🎯 Specific Objectives

- Manage projects, groups, students, and lecturers  
- Track progress through multiple review rounds  
- Automate reviewer assignment  
- Standardize evaluation using checklists  
- Provide notification & deadline reminder system  

---

## 👥 Actors

| Role | Description |
|------|------------|
| 🎓 Student | Submit documents, track progress, receive feedback |
| 👨‍🏫 Supervisor | Monitor groups, approve/reject feedback |
| 🧑‍⚖️ Reviewer | Evaluate projects using checklists |
| 🧑‍💼 Head of Department | Manage projects, schedule reviews, assign lecturers |
| ⚙️ Admin | Manage system accounts and permissions |

---

## 🔄 Business Workflow

### 1. Topic Registration & Approval
- Students propose project topics  
- Head of Department reviews and approves  
- Assign Supervisor  

### 2. Progress Review (Online)
- Create review schedule  
- Automatically assign reviewers (with constraints)  
- Students submit documents  
- Reviewers evaluate using checklists  
- Supervisor approves/rejects feedback  

### 3. Final Defense (Offline)
- Schedule defense sessions  
- Assign rooms & committees  
- Notify students and lecturers  

---

## 🚀 Key Features

- 🔐 Role-based access control  
- 📂 Project & group management  
- 📅 Review & defense scheduling  
- 🤖 Automatic reviewer assignment  
- 📊 Checklist-based evaluation system  
- 🔔 Notification & deadline tracking  
- 📈 Progress monitoring & reporting  

---

## 🏗️ System Architecture

This project follows **Clean Architecture**:
GPMS.sln
├── GPMS.Web # Presentation Layer (ASP.NET Core MVC)
├── GPMS.Application # Business Logic / Use Cases
├── GPMS.Domain # Core Entities & Interfaces
├── GPMS.Infrastructure # Data Access (EF Core, External Services)
└── GPMS.Common # Shared Utilities

---

## 🛠️ Tech Stack

### Backend
- ASP.NET Core MVC (.NET 8)  
- Entity Framework Core (Code First)  
- SQL Server  

### Frontend
- Razor Pages / MVC Views *(or React if extended)*  

### Others
- Authentication: Cookie / SSO-based  
- Notification: Email + System Notification  

---

## 🗄️ Database Design

The database is designed to:

- Support multiple faculties, majors, and specializations  
- Use **University Email as unique identifier**  
- Avoid redundant tables (no separate Student/Lecturer tables)  
- Control permissions via relationships (not hard-coded roles)  

### Main Entities:
- Users & Authentication  
- Projects & Project Groups  
- ReviewRounds & ReviewerAssignments  
- Checklists & Evaluations  
- Feedback & Approval Workflow  

> 📄 Detailed schema defined in database design

---

## 🔑 Key Design Decisions

- ❌ No over-engineered Role/Permission system  
- ✅ Permissions derived from relationships  
- ✅ Supervisor acts as academic gatekeeper  
- ✅ Database enforces business rules (not only application logic)  
- ✅ Optimized for real university workflow  

---

## 📈 Practical Value

- Solves real management problems at universities  
- Reduces manual workload for lecturers  
- Enhances transparency and accountability  
- Prevents missed deadlines and miscommunication  
- Scalable for other institutions  

---
