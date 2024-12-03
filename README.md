![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=.net&logoColor=white) ![Redis](https://img.shields.io/badge/Redis-FF4438?style=flat&logo=redis&logoColor=white) ![Cloudinary](https://img.shields.io/badge/Cloudinary-3448C5?style=flat&logo=cloudinary&logoColor=white) ![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)

# Employee Administration
.NET API for the administration of employees, projects, and their relations (tasks and members)

- Clean architecture with UoW & Services patterns
- Database & file error logging

Features:
- *JWT authentication*
- User roles *caching* (with Redis) for authorization assistance
- Cloudinary as *image storage* for user profile pictures

### API endpoints
![API endpoints](misc/endpoints.png "API endpoints")

### How to run project
- Clone project ``git clone https://github.com/stealacamera/Employee-Administration-Api.git``
- Open project in Visual Studio
- Click ``Run Docker Compose``
