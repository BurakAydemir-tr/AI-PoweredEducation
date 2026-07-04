import 'package:flutter/material.dart';

class GameCompleteScreen extends StatelessWidget {
  const GameCompleteScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Oyun Tamamlandı')),
      body: const SafeArea(
        child: Center(
          child: Text('Oyun tamamlama özeti burada gösterilecek.'),
        ),
      ),
    );
  }
}
