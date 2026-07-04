import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/routing/app_routes.dart';
import '../../../core/models/learning_task_type.dart';
import '../../../core/models/student_task_response.dart';
import '../../../core/network/api_exception.dart';
import '../../join/data/student_session_api.dart';
import '../../quiz/presentation/quiz_screen.dart';

class GameLobbyScreenArgs {
  const GameLobbyScreenArgs({required this.currentTask});

  final StudentTaskResponse currentTask;
}

class GameLobbyScreen extends ConsumerStatefulWidget {
  const GameLobbyScreen({
    this.currentTask,
    super.key,
  });

  final StudentTaskResponse? currentTask;

  @override
  ConsumerState<GameLobbyScreen> createState() => _GameLobbyScreenState();
}

class _GameLobbyScreenState extends ConsumerState<GameLobbyScreen> {
  StudentTaskResponse? _currentTask;
  bool _isLoadingProgress = false;

  @override
  void initState() {
    super.initState();
    _currentTask = widget.currentTask;

    if (_currentTask == null) {
      _loadProgress();
    }
  }

  Future<void> _loadProgress() async {
    setState(() => _isLoadingProgress = true);

    try {
      final progress = await ref.read(studentSessionApiProvider).getProgress();

      if (!mounted) {
        return;
      }

      setState(() => _currentTask = progress.currentTask);
    } on ApiException catch (exception) {
      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(exception.message)),
      );
    } finally {
      if (mounted) {
        setState(() => _isLoadingProgress = false);
      }
    }
  }

  void _startCurrentTask() {
    final task = _currentTask;

    if (task == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Başlatılacak görev bulunamadı.')),
      );
      return;
    }

    if (task.taskType != LearningTaskType.quiz) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            '${task.taskType.displayName} görevleri bu sürümde henüz desteklenmiyor.',
          ),
        ),
      );
      return;
    }

    context.go(
      AppRoutes.quiz,
      extra: QuizScreenArgs(
        task: task,
        totalTaskCount: 10,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final currentTaskType = _currentTask?.taskType.displayName;

    return Scaffold(
      appBar: AppBar(title: const Text('Oyun Lobisi')),
      body: SafeArea(
        child: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.check_circle_outline,
                  color: theme.colorScheme.primary,
                  size: 64,
                ),
                const SizedBox(height: 16),
                Text(
                  'Oyuna başarıyla katıldın.',
                  style: theme.textTheme.headlineSmall,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                Text(
                  'Sıradaki görevi bu ekrandan başlatabilirsin.',
                  style: theme.textTheme.bodyLarge,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 24),
                if (_isLoadingProgress)
                  const CircularProgressIndicator()
                else if (_currentTask == null)
                  const Text(
                    'Sıradaki görev yüklenemedi. Oyuna tekrar katılmayı dene.',
                    textAlign: TextAlign.center,
                  )
                else ...[
                  Text(
                    'Sıradaki görev: $currentTaskType',
                    style: theme.textTheme.titleMedium,
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),
                  FilledButton(
                    onPressed: _startCurrentTask,
                    child: const Text('Oyuna Başla'),
                  ),
                  if (_currentTask!.taskType != LearningTaskType.quiz) ...[
                    const SizedBox(height: 12),
                    const Text(
                      'Bu aşamada yalnızca Quiz görevleri oynanabilir.',
                      textAlign: TextAlign.center,
                    ),
                  ],
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
