import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/routing/app_routes.dart';
import '../../../app/theme/app_colors.dart';
import '../../../core/models/learning_task_type.dart';
import '../../../core/models/quiz_answer_option.dart';
import '../../../core/models/student_task_response.dart';
import '../../../core/network/api_exception.dart';
import '../../game_lobby/presentation/game_lobby_screen.dart';
import '../data/quiz_api.dart';
import '../data/submit_quiz_answer_request.dart';

class QuizScreenArgs {
  const QuizScreenArgs({
    required this.task,
    required this.totalTaskCount,
  });

  final StudentTaskResponse task;
  final int totalTaskCount;
}

class QuizScreen extends ConsumerStatefulWidget {
  const QuizScreen({
    required this.task,
    required this.totalTaskCount,
    super.key,
  });

  final StudentTaskResponse task;
  final int totalTaskCount;

  @override
  ConsumerState<QuizScreen> createState() => _QuizScreenState();
}

class _QuizScreenState extends ConsumerState<QuizScreen> {
  QuizAnswerOption? _selectedAnswer;
  QuizAnswerOption? _revealedCorrectAnswer;
  StudentTaskResponse? _nextTask;
  String? _message;
  int? _earnedScore;
  int _wrongAttemptCount = 0;
  bool _isSubmitting = false;
  bool _isTaskResolved = false;

  @override
  void didUpdateWidget(covariant QuizScreen oldWidget) {
    super.didUpdateWidget(oldWidget);

    if (oldWidget.task.id != widget.task.id) {
      _resetTaskState();
    }
  }

  void _resetTaskState() {
    _selectedAnswer = null;
    _revealedCorrectAnswer = null;
    _nextTask = null;
    _message = null;
    _earnedScore = null;
    _wrongAttemptCount = 0;
    _isSubmitting = false;
    _isTaskResolved = false;
  }

  Future<void> _submitAnswer(QuizAnswerOption answer) async {
    if (_isSubmitting || _isTaskResolved) {
      return;
    }

    setState(() {
      _selectedAnswer = answer;
      _isSubmitting = true;
      _message = null;
    });

    try {
      final progress = await ref.read(quizApiProvider).submitAnswer(
            SubmitQuizAnswerRequest(answer: answer),
          );

      if (!mounted) {
        return;
      }

      final revealedCorrectAnswer = progress.revealedCorrectAnswer;
      final didAdvance = progress.currentTask?.id != widget.task.id;

      if (revealedCorrectAnswer != null) {
        setState(() {
          _wrongAttemptCount = 3;
          _revealedCorrectAnswer = revealedCorrectAnswer;
          _nextTask = progress.currentTask;
          _earnedScore = 0;
          _message = 'Doğru cevap: ${_answerLabel(revealedCorrectAnswer)}';
          _isTaskResolved = true;
        });
        return;
      }

      if (didAdvance) {
        setState(() {
          _nextTask = progress.currentTask;
          _earnedScore = _scoreForCorrectAnswer();
          _message = 'Cevap doğru. $_earnedScore puan kazandın.';
          _isTaskResolved = true;
        });
        return;
      }

      setState(() {
        _wrongAttemptCount += 1;
        _message = 'Cevap yanlış. Bir daha denemelisin.';
      });
    } on ApiException catch (exception) {
      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(exception.message)),
      );
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
      }
    }
  }

  int _scoreForCorrectAnswer() {
    return (100 - (_wrongAttemptCount * 25)).clamp(0, 100);
  }

  String _answerLabel(QuizAnswerOption answer) {
    final optionText = widget.task.optionTextFor(answer);
    return '${answer.label}. $optionText';
  }

  void _goToNextStep() {
    final nextTask = _nextTask;

    if (nextTask == null) {
      context.go(AppRoutes.gameComplete);
      return;
    }

    if (nextTask.taskType == LearningTaskType.quiz) {
      context.go(
        AppRoutes.quiz,
        extra: QuizScreenArgs(
          task: nextTask,
          totalTaskCount: widget.totalTaskCount,
        ),
      );
      return;
    }

    context.go(
      AppRoutes.gameLobby,
      extra: GameLobbyScreenArgs(currentTask: nextTask),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Quiz Görevi')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _TaskProgress(
                currentTaskOrder: widget.task.order,
                totalTaskCount: widget.totalTaskCount,
              ),
              const SizedBox(height: 24),
              Text(
                widget.task.question ?? '',
                style: theme.textTheme.headlineSmall,
              ),
              const SizedBox(height: 24),
              ...QuizAnswerOption.values.map(
                (answer) => Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: _AnswerButton(
                    answer: answer,
                    text: widget.task.optionTextFor(answer),
                    selectedAnswer: _selectedAnswer,
                    correctAnswer: _revealedCorrectAnswer,
                    disabled: _isSubmitting || _isTaskResolved,
                    onPressed: () => _submitAnswer(answer),
                  ),
                ),
              ),
              if (_message != null) ...[
                const SizedBox(height: 12),
                _QuizFeedback(
                  message: _message!,
                  isSuccess: _earnedScore != null && _earnedScore! > 0,
                  isFailure: _revealedCorrectAnswer != null,
                  earnedScore: _earnedScore,
                ),
              ],
              if (_isTaskResolved) ...[
                const SizedBox(height: 24),
                FilledButton(
                  onPressed: _goToNextStep,
                  child: Text(
                    _nextTask == null ? 'Oyunu Tamamla' : 'Sonraki Göreve Geç',
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _TaskProgress extends StatelessWidget {
  const _TaskProgress({
    required this.currentTaskOrder,
    required this.totalTaskCount,
  });

  final int currentTaskOrder;
  final int totalTaskCount;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        Text(
          '$currentTaskOrder/$totalTaskCount',
          style: theme.textTheme.titleMedium,
        ),
        const SizedBox(width: 12),
        Expanded(
          child: LinearProgressIndicator(
            value: totalTaskCount == 0 ? 0 : currentTaskOrder / totalTaskCount,
          ),
        ),
      ],
    );
  }
}

class _AnswerButton extends StatelessWidget {
  const _AnswerButton({
    required this.answer,
    required this.text,
    required this.selectedAnswer,
    required this.correctAnswer,
    required this.disabled,
    required this.onPressed,
  });

  final QuizAnswerOption answer;
  final String text;
  final QuizAnswerOption? selectedAnswer;
  final QuizAnswerOption? correctAnswer;
  final bool disabled;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    final isSelected = selectedAnswer == answer;
    final isCorrect = correctAnswer == answer;

    return OutlinedButton(
      onPressed: disabled ? null : onPressed,
      style: OutlinedButton.styleFrom(
        alignment: Alignment.centerLeft,
        minimumSize: const Size.fromHeight(56),
        side: BorderSide(
          color: isCorrect
              ? AppColors.success
              : isSelected
                  ? Theme.of(context).colorScheme.primary
                  : Theme.of(context).colorScheme.outline,
        ),
      ),
      child: Text('${answer.label}. $text'),
    );
  }
}

class _QuizFeedback extends StatelessWidget {
  const _QuizFeedback({
    required this.message,
    required this.isSuccess,
    required this.isFailure,
    required this.earnedScore,
  });

  final String message;
  final bool isSuccess;
  final bool isFailure;
  final int? earnedScore;

  @override
  Widget build(BuildContext context) {
    final color = isSuccess
        ? AppColors.success
        : isFailure
            ? AppColors.danger
            : AppColors.warning;

    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(color: color),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              message,
              style:
                  Theme.of(context).textTheme.bodyLarge?.copyWith(color: color),
            ),
            if (earnedScore != null) ...[
              const SizedBox(height: 8),
              Text('Kazanılan puan: $earnedScore'),
            ],
          ],
        ),
      ),
    );
  }
}
