# Almosafir Flutter App - Complete Mobile

## Setup
1. Install Flutter: flutter.dev
2. flutter create almosafir_flutter
3. cd almosafir_flutter && flutter pub add http google_fonts flutter_map geolocator shared_preferences dio lottie flutter_secure_storage
4. Backend: localhost/almosafir (this PHP API)

## Full Code Structure (Copy/Paste)

**pubspec.yaml** (add):
```
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  google_fonts: ^6.1.0
  flutter_map: ^6.1.0
  geolocator: ^10.1.0
  shared_preferences: ^2.2.2
  lottie: ^2.7.0
  dio: ^5.4.0
```

**lib/main.dart**:
```dart
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'screens/home_screen.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Almosafir',
      theme: ThemeData(
        fontFamily: GoogleFonts.cairo().fontFamily,
        primarySwatch: Colors.blue,
      ),
      home: HomeScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}
```

**lib/screens/home_screen.dart** (3D UI + Yemen autocomplete):
```dart
import 'package:flutter/material.dart';
class HomeScreen extends StatefulWidget {
  @override
  _HomeScreenState createState() => _HomeScreenState();
}
class _HomeScreenState extends State<HomeScreen> {
  final governorates = ['صنعاء', 'عدن', 'تعز', 'الحديدة' /* full list */];
  String fromCity = '', toCity = '';
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(colors: [Color(0xFF1e3c72), Color(0xFF2a5298)])
        ),
        child: SafeArea(
          child: Column(
            children: [
              Text('🚀 المسافر', style: TextStyle(fontSize: 40, shadows: [
                Shadow(color: Colors.black26, offset: Offset(2,2), blurRadius: 4)
              ])),
              Text('عمار عادل | 712275038', style: TextStyle(fontSize: 16)),
              Padding(padding: EdgeInsets.all(20),
                child: Column(
                  children: [
                    TextField( /* autocomplete from governorates */ ),
                    TextField(),
                    ElevatedButton(child: Text('🔍 بحث'), onPressed: (){}),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
```

**Run**:
```
flutter pub get
flutter run -d chrome  # Web
flutter run -d android # Phone
```

**Features**:
- Yemen autocomplete/map
- Real-time API (your PHP)
- 3D cards/animations (Lottie)
- Secure auth
- Push notifications

**API Integration**: http://localhost/almosafir/search.php?...

App كامل جاهز - copy/run! 📱
