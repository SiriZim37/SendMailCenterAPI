# SendMailAPI - C# Email Sending Service

**SendMailAPI** is a simple C# application designed to send emails using SMTP. The application supports sending HTML emails, including **CC**, **BCC**, and **attachments** (encoded in Base64). It also allows sending emails asynchronously for better performance, especially when handling multiple email requests or large attachments.

This project can be used to integrate email functionality into any C# application, such as for sending user notifications, alerts, or other types of communications.

## Features

- **Asynchronous Email Sending**: Non-blocking operations for improved performance.
- **SMTP Configuration**: Easily configurable SMTP server settings.
- **HTML Email Support**: Send emails with rich content (e.g., images, links).
- **CC and BCC Support**: Allows adding CC and BCC recipients.
- **Attachment Support**: Attach files encoded in Base64 format.
- **Error Handling**: Provides detailed error messages in case of failures.

## Prerequisites

Before using the **SendMailAPI** project, ensure you have the following:

- **.NET Core SDK** installed on your machine.
- A valid SMTP server (e.g., Gmail, Outlook, etc.).
- **AppSettings** configured with SMTP credentials.

## Installation

Follow these steps to install and run the project:

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/SendMailAPI.git
## Steps

1. **Navigate to the project directory:**
    ```bash
    cd SendMailAPI
    ```

2. **Install the required dependencies:**
    ```bash
    dotnet restore
    ```

3. **Set up the SMTP configuration in the `appsettings.json` file. Example:**
    ```json
    {
      "MailSettings": {
        "Host": "smtp.example.com",
        "Port": 587,
        "UserName": "your-email@example.com",
        "Password": "your-smtp-password"
      }
    }
    ```

4. **Build the project:**
    ```bash
    dotnet build
    ```

5. **Run the application:**
    ```bash
    dotnet run
    ```

## Configuration

The email settings are loaded from the `appsettings.json` file using dependency injection. Below is an example configuration for the `MailSettings`:

```json
{
  "MailSettings": {
    "Host": "smtp.your-email-provider.com",
    "Port": 587,
    "UserName": "your-email@example.com",
    "Password": "your-password"
  }
}
