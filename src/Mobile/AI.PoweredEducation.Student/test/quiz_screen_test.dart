import 'package:ai_powered_education_student/core/models/learning_task_type.dart';
import 'package:ai_powered_education_student/core/models/student_task_response.dart';
import 'package:ai_powered_education_student/features/quiz/presentation/quiz_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() {
  testWidgets('tek quiz görevini ilerleme ve cevaplarla gösterir', (
    tester,
  ) async {
    const task = StudentTaskResponse(
      id: 'task-1',
      taskType: LearningTaskType.quiz,
      order: 1,
      question: 'What is 2 + 2?',
      optionA: '3',
      optionB: '4',
      optionC: '5',
      optionD: '6',
    );

    await tester.pumpWidget(
      const ProviderScope(
        child: MaterialApp(
          home: QuizScreen(
            task: task,
            totalTaskCount: 10,
          ),
        ),
      ),
    );

    expect(find.text('1/10'), findsOneWidget);
    expect(find.text('What is 2 + 2?'), findsOneWidget);
    expect(find.text('A. 3'), findsOneWidget);
    expect(find.text('B. 4'), findsOneWidget);
    expect(find.text('C. 5'), findsOneWidget);
    expect(find.text('D. 6'), findsOneWidget);
  });
}
