import { useState } from 'react';

const MAX_LENGTH = 250;

/** Zone de saisie d'un message. Entrée = envoyer, Shift+Entrée = nouvelle ligne. */
export default function MessageComposer({ onSend, disabled }) {
  const [value, setValue] = useState('');

  function submit() {
    const content = value.trim();
    if (!content || disabled) return;
    onSend(content);
    setValue('');
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      submit();
    }
  }

  return (
    <div className="border-t border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-900">
      <div className="flex items-end gap-3">
        <textarea
          className="input max-h-40 min-h-[46px] flex-1 resize-none"
          rows={1}
          placeholder={disabled ? 'Sélectionnez une conversation…' : 'Écrire un message…'}
          value={value}
          maxLength={MAX_LENGTH}
          disabled={disabled}
          onChange={(e) => setValue(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <button
          type="button"
          className="btn-primary"
          onClick={submit}
          disabled={disabled || !value.trim()}
        >
          Envoyer
        </button>
      </div>
      {value.length > MAX_LENGTH - 30 && (
        <p className="mt-1 text-right text-xs text-slate-400 dark:text-slate-500">
          {value.length}/{MAX_LENGTH}
        </p>
      )}
    </div>
  );
}
