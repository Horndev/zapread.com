/*
 * React component for the top header and breadcrumb
 */

import React from 'react';

class PageHeading extends React.Component {
    render() {
        return (
            <div className="wrapper border-bottom white-bg page-heading">
                <div className="col-lg-10">
                    <br /><h2>{this.props.title}</h2>
                    <ol className="breadcrumb">
                        <li className="breadcrumb-item"><a href="/">{this.props.controller}</a></li>
                        <li className="breadcrumb-item"><a href="/">{this.props.method}</a></li>
                        <li className="breadcrumb-item active">{this.props.function}</li>
                    </ol>
                </div>
                <div className="col-lg-2" />
            </div>
        );
    }
}

export default PageHeading;