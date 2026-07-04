import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/routing/app_routes.dart';
import '../../../app/theme/app_colors.dart';
import '../../../core/network/api_exception.dart';
import '../../game_lobby/presentation/game_lobby_screen.dart';
import '../data/join_game_request.dart';
import '../data/student_session_api.dart';

class JoinScreen extends ConsumerStatefulWidget {
  const JoinScreen({super.key});

  @override
  ConsumerState<JoinScreen> createState() => _JoinScreenState();
}

class _JoinScreenState extends ConsumerState<JoinScreen> {
  final _formKey = GlobalKey<FormState>();
  final _gameCodeController = TextEditingController();
  final _studentNameController = TextEditingController();

  bool _isSubmitting = false;
  String? _errorMessage;

  @override
  void dispose() {
    _gameCodeController.dispose();
    _studentNameController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() {
      _isSubmitting = true;
      _errorMessage = null;
    });

    try {
      final joinResponse = await ref.read(studentSessionApiProvider).join(
            JoinGameRequest(
              gameCode: _gameCodeController.text.trim().toUpperCase(),
              studentName: _studentNameController.text.trim(),
            ),
          );

      if (!mounted) {
        return;
      }

      context.go(
        AppRoutes.gameLobby,
        extra: GameLobbyScreenArgs(currentTask: joinResponse.currentTask),
      );
    } on ApiException catch (exception) {
      if (!mounted) {
        return;
      }

      setState(() => _errorMessage = exception.message);
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(
                      'Oyuna Katıl',
                      style: theme.textTheme.headlineMedium,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Oyun kodunu ve öğrenci adını gir.',
                      style: theme.textTheme.bodyLarge,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 32),
                    TextFormField(
                      controller: _gameCodeController,
                      decoration: const InputDecoration(
                        labelText: 'Oyun Kodu',
                      ),
                      textCapitalization: TextCapitalization.characters,
                      inputFormatters: [
                        FilteringTextInputFormatter.allow(
                          RegExp('[a-zA-Z0-9]'),
                        ),
                        LengthLimitingTextInputFormatter(6),
                      ],
                      validator: (value) {
                        final gameCode = value?.trim() ?? '';
                        if (gameCode.isEmpty) {
                          return 'Oyun kodu zorunludur.';
                        }
                        if (gameCode.length != 6) {
                          return 'Oyun kodu 6 karakter olmalıdır.';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 16),
                    TextFormField(
                      controller: _studentNameController,
                      decoration: const InputDecoration(
                        labelText: 'Öğrenci Adı',
                      ),
                      textInputAction: TextInputAction.done,
                      onFieldSubmitted: (_) {
                        if (!_isSubmitting) {
                          _submit();
                        }
                      },
                      validator: (value) {
                        final studentName = value?.trim() ?? '';
                        if (studentName.isEmpty) {
                          return 'Öğrenci adı zorunludur.';
                        }
                        return null;
                      },
                    ),
                    if (_errorMessage != null) ...[
                      const SizedBox(height: 16),
                      _JoinErrorMessage(message: _errorMessage!),
                    ],
                    const SizedBox(height: 24),
                    FilledButton(
                      onPressed: _isSubmitting ? null : _submit,
                      child: _isSubmitting
                          ? const SizedBox.square(
                              dimension: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Katıl'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _JoinErrorMessage extends StatelessWidget {
  const _JoinErrorMessage({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.danger.withValues(alpha: 0.08),
        border: Border.all(color: AppColors.danger),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Icon(
              Icons.error_outline,
              color: AppColors.danger,
              size: 20,
            ),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                message,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: AppColors.danger,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
