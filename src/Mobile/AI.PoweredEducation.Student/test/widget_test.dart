import 'package:ai_powered_education_student/app/student_app.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  testWidgets('katılım ekranını gösterir', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: StudentApp()));

    expect(find.text('Oyuna Katıl'), findsOneWidget);
    expect(find.text('Oyun Kodu'), findsOneWidget);
    expect(find.text('Öğrenci Adı'), findsOneWidget);
    expect(find.text('Katıl'), findsOneWidget);
  });
}
