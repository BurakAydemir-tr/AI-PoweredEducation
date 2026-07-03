import 'package:ai_powered_education_student/app/student_app.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  testWidgets('shows the join screen', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: StudentApp()));

    expect(find.text('Join Learning Game'), findsOneWidget);
    expect(find.text('Game Code'), findsOneWidget);
    expect(find.text('Student Name'), findsOneWidget);
  });
}
