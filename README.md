# Cosmetics Store Management Application

This project is a comprehensive cosmetics store management application designed to handle products, customer interactions, and financial tracking in one unified system.

## 🌟 Key Features

### 🛍️ Product Management
*   **Browse Products:** Users can explore the catalog of available cosmetics.
*   **Reserve Items:** Customers have the ability to reserve items they intend to purchase.
*   **Inventory Control:** Staff/admins can seamlessly add new products to the catalog and update details for existing ones.

### 👥 Customer Management
*   **Google Authentication:** Secure and easy customer registration and login using Google accounts.
*   **Customer Directory:** View a comprehensive list of registered customers.
*   **Profile Updates:** Manage and update customer information as needed.

### 💰 Financial & Debt Management
*   **Debt Tracking:** Dedicated feature to meticulously manage store debts.
*   **Creditors & Debtors:** Keep track of who owes the store money (debtors) and who the store owes money to (creditors).
*   **Transaction History:** Store permanent debt records and view detailed logs of all financial transactions related to these debts.

## 📂 Project Structure

*   `StoreBackend/`: Contains the core backend service, including the API, database models, and business logic.
*   `Store-deployment/`: Contains the Docker orchestration files (e.g., `docker-compose.yml`) used to deploy the application and its dependencies (like the database).
*   `Store-frontend/`: Contains the Angular-based frontend web application.
