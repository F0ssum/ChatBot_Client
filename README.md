# ChatBotClient Project Structure

- **Core/**: Common models, interfaces, converters, and configuration.
  - **Models/**: Data models (Message, DiaryEntry, User).
  - **Interfaces/**: Service and ViewModel interfaces.
  - **Converters/**: WPF value converters.
  - **Configuration/**: Application configuration (AppConfiguration).
- **Features/**: Feature-specific logic and UI.
  - **Chat/**: Chat-related ViewModels and Views.
  - **Diary/**: Diary-related ViewModels and Views.
  - **Settings/**: Settings-related ViewModels and Views.
  - ...
- **ViewModels/Settings/**: ViewModels for settings (ChatSettingsViewModel, etc.).
- **Infrastructure/**: Cross-cutting services (caching, storage, notifications).
- **Resources/**: Styles, sounds, and icons.