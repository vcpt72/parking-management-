# 🅿️ Parking Management System

A desktop application for managing parking lots, including tracking parking spots, their statuses, and usage statistics.

## 📚 Technologies Used

- ASP.NET Core MVC (Web)
- Dapper 
- Bootstrap
- SQLite (Embedded database)
- WPF (.NET 6/7) (Desktop UI)
- HTTP API communication between WPF and ASP.NET Core API

---

## 🌐 MVC Web Application Functionality

### Authentication
- Simple login and registration (no password hashing – demo purpose only)
- Two roles: **Admin** and **User**
- admin login: admin admin123
- testuser login: test test
### Admin Capabilities
- Manage parking lots and their spots:
  - ✅ Add a new parking spot
  - ❌ Remove a parking spot
  - 🔄 Change spot status to **Available** or **Maintenance**
  - 📜 View parking spot status history

### User Capabilities
- View parking lots and available spots
- Occupy a parking spot


## 💻 WPF Desktop Application Functionality

- Admin-only functionality (no login or registration)
- Connected via HTTP API to the ASP.NET backend
- Features:
  - View all parking lots
  - View parking lot details (name, coordinates, statistics)
  - ✅ Add new spot
  - ❌ Remove spot
  - 🔄 Change status to **Available** or **Maintenance**
  - 📜 View history of a spot in a new window

 ## ⚠️ Notes

- This is a demo project — passwords are stored in plain text (not secure!)
- SQLite is used for simplicity; for production use, consider PostgreSQL or SQL Server
- No token-based authentication or authorization on the WPF side
- Easily extensible with authentication, filters, pagination, or role-based API protection
- You need to run both apps (MVC,WPF) for WPF to be functional
---

## 📦 API Endpoints Overview

### ParkingLots
- `GET /parkinglots` — list all parking lots
- `GET /parkinglots/{id}` — parking lot details (including spots)
- `GET /parkinglots/{id}/stats` — monthly usage stats

### ParkingSpots
- `POST /parkinglots/{parkingLotId}/parkingspots` — add spot
- `DELETE /parkinglots/{parkingLotId}/removespot/{spotId}` — delete spot
- `PUT /parkinglots/{parkingLotId}/spot/{spotId}/maintenance` — set to Maintenance
- `PUT /parkinglots/{parkingLotId}/spot/{spotId}/available` — set to Available

### Status History
- `GET /parkingspots/{spotId}/history` — status change log

---

## 🧪 Helper Class: `ApiService` (WPF)

Includes methods for:
- `GetParkingLotByIdAsync(id)`
- `AddParkingSpotAsync(parkingLotId, spot)`
- `RemoveParkingSpotAsync(parkingLotId, spotId)`
- `SetSpotToMaintenanceAsync(parkingLotId, spotId)`
- `SetSpotToAvailableAsync(parkingLotId, spotId)`
- `GetParkingSpotHistoryAsync(spotId)`

## MVC Showcase
### Admin
![image](https://github.com/user-attachments/assets/b517353d-5eeb-4295-93ef-d8d73fcbcf56)
![image](https://github.com/user-attachments/assets/de657879-6a8d-4fb0-9722-613b6897fe44)
![image](https://github.com/user-attachments/assets/70bc26fa-841b-481b-af71-9d6254f28afb)

### User
![image](https://github.com/user-attachments/assets/f4f2e297-c17f-4ae4-a282-22a2f904e26d)
![image](https://github.com/user-attachments/assets/96b9c41f-84a8-4401-8ee7-eb2844570527)


## WPF Showcase
![image](https://github.com/user-attachments/assets/62da4574-019a-4079-8450-9f62658678d4)
![image](https://github.com/user-attachments/assets/baf96d65-8ac9-4dbf-90fe-d236a4fd80e4)
![image](https://github.com/user-attachments/assets/d5cbe2f9-be8e-4999-ad1b-a78655f6b2ee)

