import 'package:flutter/material.dart';

class GameCompleteScreen extends StatelessWidget {
  const GameCompleteScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Game Complete')),
      body: const SafeArea(
        child: Center(
          child: Text('Game complete summary will be shown here.'),
        ),
      ),
    );
  }
}
