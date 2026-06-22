import React, { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';
import { Icons } from '../../shared/components/lms/Icons';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class StudioErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null
  };

  public static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('LMS Studio Fatal Exception:', error, errorInfo);
  }

  private handleReset = () => {
    window.location.reload();
  };

  public render() {
    if (this.state.hasError) {
      return (
        <div className="lms-studio-frame" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--color-bg)' }}>
          <div className="lms-premium-card" style={{ maxWidth: '600px', textAlign: 'center', padding: '60px', border: '1px solid var(--color-danger-glow)' }}>
            <div className="lms-status-icon danger" style={{ width: '80px', height: '80px', margin: '0 auto 32px' }}>
                <Icons.Shield s={40} />
            </div>
            <h1 className="lms-studio-title" style={{ fontSize: '28px', marginBottom: '16px' }}>Studio Critical Interruption</h1>
            <p className="lms-status-sub" style={{ fontSize: '14px', marginBottom: '32px', maxWidth: '400px', margin: '0 auto 32px' }}>
              The application encountered an unexpected state partition. This has been logged for architectural review.
            </p>
            
            <div className="lms-form-accent-box" style={{ background: 'rgba(239, 68, 68, 0.05)', border: '1px solid rgba(239, 68, 68, 0.1)', padding: '16px', marginBottom: '32px', textAlign: 'left' }}>
                <code style={{ fontSize: '12px', color: 'var(--color-danger)' }}>
                    [{this.state.error?.name}] {this.state.error?.message}
                </code>
            </div>

            <div className="lms-flex-row" style={{ justifyContent: 'center', gap: '16px' }}>
                <button 
                onClick={() => this.setState({ hasError: false, error: null })} 
                className="lms-btn"
                >
                RECOVERY ATTEMPT
                </button>
                <button 
                onClick={this.handleReset} 
                className="lms-btn-primary danger"
                >
                RELINK STUDIO
                </button>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
